using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class BriefingView : MonoBehaviour, IBriefingScreen
    {
        const float RefusalHoldFactor = 4f;
        const float FallbackViewWidth = 20f;
        const float FallbackViewHeight = 11.25f;

        enum Mode
        {
            None,
            Letter,
            Report,
            Review
        }

        OverworldSettings settings;
        Func<string, CaseFile> findCase;
        Transform deskRoot;
        TypewriterSound sound;
        PaperView descriptionPaper;
        PaperView letterPaper;
        Tween slideTween;
        Tween appearTween;
        Tween fadeTween;
        Keyboard keyboard;
        BriefingGate gate;
        DivergingTyper letterTyper;
        LetterPaperModel letterModel;
        StoryTyper responseTyper;
        ResponsePaperModel responseModel;
        PaperReveal leftReveal;
        LineScales leftScales = LineScales.Uniform;
        TextRole leftSettledRole;
        TextRole leftRevealedRole;
        Action<LetterOutcome> letterFinished;
        Action reportFinished;
        Action reviewFinished;
        Mode mode;
        Vector3 deskHome;
        float slide;
        float letterHidden = 1f;
        float unchosenAlpha = 1f;
        float fadeTarget = 1f;
        float viewWidth;
        bool listening;
        int generation;

        public void Configure(OverworldSettings overworldSettings, Func<string, CaseFile> caseLookup, Transform slidingDesk, TypewriterSound typewriterSound)
        {
            settings = overworldSettings;
            findCase = caseLookup;
            deskRoot = slidingDesk;
            sound = typewriterSound;
            descriptionPaper = NewPaper("Description Paper", settings.DescriptionPaperSprite);
            letterPaper = NewPaper("Letter Paper", settings.LetterPaperSprite);
        }

        public void Open(string caseId, Action<LetterOutcome> finishedCallback)
        {
            var file = findCase(caseId);
            mode = Mode.Letter;
            letterFinished = finishedCallback;
            UseRoles(PaperScreen.Letter);
            descriptionPaper.UseRequestFont(file.DescriptionFontOverride);
            var leftBlocks = new[] { new TextBlock(file.ResolvedDescription, 1f) };
            var letters = PlacePapers(file.name, leftBlocks, LetterPaperModel.Unwrapped(file.ClientName, file.ResolvedAcceptMessage, file.ResolvedDeclineMessage));
            leftReveal = new PaperReveal(string.Empty, TextWrap.Wrap(file.ResolvedDescription, letters.Description));
            leftScales = LineScales.Uniform;
            ResetUnchosenFade();
            letterTyper = new DivergingTyper(file.ResolvedAcceptLetter, file.ResolvedDeclineLetter, settings.TypingRules);
            letterModel = new LetterPaperModel(file.ClientName, file.ResolvedAcceptMessage, file.ResolvedDeclineMessage, letters.Letter);
            gate = new BriefingGate(new TimedReveal(leftReveal.RevealedLength, settings.DescriptionSecondsPerCharacter, settings.DescriptionSkipSpeedMultiplier));
            ShowLeft(0);
            RenderLetter();
            Begin();
        }

        public void OpenReport(string caseId, Action finishedCallback)
        {
            var file = findCase(caseId);
            mode = Mode.Report;
            reportFinished = finishedCallback;
            UseRoles(PaperScreen.Report);
            descriptionPaper.UseRequestFont(file.DescriptionFontOverride);
            var letters = PlacePapers(file.name, ReportBlocks(file), file.ResolvedResponse);
            UseReportReveal(file, letters);
            gate = new BriefingGate(new TimedReveal(leftReveal.RevealedLength, settings.DescriptionSecondsPerCharacter, settings.DescriptionSkipSpeedMultiplier));
            responseTyper = new StoryTyper(file.ResolvedResponse, settings.TypingRules, settings.ResponseUnit);
            responseModel = new ResponsePaperModel(responseTyper.Text, letters.Letter);
            ShowLeft(0);
            RenderResponse();
            Begin();
        }

        public void OpenReview(string caseId, Action finishedCallback)
        {
            var file = findCase(caseId);
            mode = Mode.Review;
            reviewFinished = finishedCallback;
            UseRoles(PaperScreen.Review);
            descriptionPaper.UseRequestFont(file.DescriptionFontOverride);
            var letters = PlacePapers(file.name, ReportBlocks(file), file.ResolvedResponse);
            UseReportReveal(file, letters);
            gate = new BriefingGate(new TimedReveal(0, settings.DescriptionSecondsPerCharacter, settings.DescriptionSkipSpeedMultiplier));
            responseModel = new ResponsePaperModel(file.ResolvedResponse, letters.Letter);
            ShowLeft(leftReveal.RevealedLength);
            RenderReview();
            Begin();
        }

        void UseRoles(PaperScreen screen)
        {
            var roles = PaperScreens.RolesOf(screen);
            leftSettledRole = roles.LeftSettled;
            leftRevealedRole = roles.LeftRevealed;
        }

        TextBlock[] ReportBlocks(CaseFile file) => new[] { new TextBlock(file.ResolvedDescription, 1f), new TextBlock(file.ResolvedFindings, settings.FindingsSizeShare) };

        void UseReportReveal(CaseFile file, PaperLetters letters)
        {
            string description = TextWrap.Wrap(file.ResolvedDescription, letters.Description);
            string findings = TextWrap.Wrap(file.ResolvedFindings, letters.Findings);
            leftReveal = new PaperReveal(description, findings);
            leftScales = LineScales.Report(description, findings, settings.FindingsSizeShare);
        }

        public void Refuse(string message, Action finished)
        {
            letterPaper.SetText(message, FontRoles.Fill(message.Length, TextRole.Notice), settings.PendingTextColor);
            FinishAfterPause(finished, RefusalHoldFactor);
        }

        public void Close(Action done)
        {
            StopListening();
            generation++;
            appearTween?.Kill();
            fadeTween?.Kill();
            SlideTo(0f, () => FinishClose(done));
        }

        void Update()
        {
            if (!listening) return;
            if (keyboard.anyKey.wasPressedThisFrame) gate.OnAnyKey();
            bool wasWritable = gate.IsWritable;
            int revealed = gate.Tick(Time.deltaTime);
            if (revealed > 0) RevealLeftWithSound();
            if (!wasWritable && gate.IsWritable) ShowLetterPaper();
            if (mode == Mode.Review && wasWritable && keyboard.anyKey.wasPressedThisFrame) FinishReview();
            if (gate.IsWritable && mode != Mode.Review && keyboard.backspaceKey.wasPressedThisFrame) Backspace();
        }

        void OnDisable()
        {
            StopListening();
            slideTween?.Kill();
            appearTween?.Kill();
            fadeTween?.Kill();
        }

        PaperView NewPaper(string paperName, Sprite sprite)
        {
            var paper = new GameObject(paperName).AddComponent<PaperView>();
            paper.transform.SetParent(transform, false);
            paper.Configure(sprite, settings);
            return paper;
        }

        readonly struct PaperLetters
        {
            public PaperLetters(int description, int findings, int letter)
            {
                Description = description;
                Findings = findings;
                Letter = letter;
            }

            public int Description { get; }

            public int Findings { get; }

            public int Letter { get; }
        }

        PaperLetters PlacePapers(string caseName, TextBlock[] leftBlocks, string rightText)
        {
            var view = ViewSize();
            var centre = ViewCentre();
            viewWidth = view.x;
            var descriptionSpot = BriefingLayout.Place(settings.DescriptionPlacement, centre, view);
            var letterSpot = BriefingLayout.Place(settings.LetterPlacement, centre, view);
            var left = FitText(descriptionPaper, descriptionSpot, leftBlocks, view.y, caseName);
            var right = FitText(letterPaper, letterSpot, new[] { new TextBlock(rightText, 1f) }, view.y, caseName);
            descriptionPaper.Place(descriptionSpot, TextGridOf(left), settings.PaperPaddingShare);
            letterPaper.Place(letterSpot, TextGridOf(right), settings.PaperPaddingShare);
            float areaWidth = descriptionPaper.TextArea(descriptionSpot.Width, settings.PaperPaddingShare).x;
            int findingsLetters = TextFit.LettersPerLine(areaWidth, left.LineHeight * settings.FindingsSizeShare, settings.TextAdvanceRatio);
            return new PaperLetters(left.LettersPerLine, findingsLetters, right.LettersPerLine);
        }

        TextGrid TextGridOf(TextFitResult fit) => new TextGrid(fit.LineHeight, settings.TextLineHeightRatio, settings.TextAdvanceRatio);

        TextFitResult FitText(PaperView paper, PaperSpot spot, TextBlock[] blocks, float viewHeight, string caseName)
        {
            var area = paper.TextArea(spot.Width, settings.PaperPaddingShare);
            var result = TextFit.Fit(blocks, area, viewHeight * settings.TextHeightShare, viewHeight * settings.HardMinTextHeightShare, viewHeight * settings.TextFitStepShare, settings.TextAdvanceRatio, settings.TextLineHeightRatio);
            if (!result.Fits) Debug.LogWarning("Case '" + caseName + "': the text on the " + paper.name + " does not fit even at the hard minimum size. Shorten it, make the paper bigger or lower Hard Min Text Height Share.");
            return result;
        }

        void Begin()
        {
            deskHome = deskRoot.position;
            slide = 0f;
            letterHidden = 1f;
            descriptionPaper.SetVisible(true);
            letterPaper.SetVisible(false);
            ApplySlide(0f);
            SlideTo(1f, StartListening);
        }

        void ShowLetterPaper()
        {
            letterPaper.SetVisible(true);
            appearTween?.Kill();
            appearTween = DOVirtual.Float(1f, 0f, settings.LetterAppearSeconds, ApplyLetterHidden)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject);
        }

        void ApplyLetterHidden(float value)
        {
            letterHidden = value;
            PlaceLetterPaper();
        }

        void SlideTo(float target, Action done)
        {
            slideTween?.Kill();
            slideTween = DOVirtual.Float(slide, target, settings.SlideSeconds, ApplySlide)
                .SetEase(Ease.InOutCubic)
                .SetLink(gameObject)
                .OnComplete(() => FinishSlide(done));
        }

        void FinishSlide(Action done)
        {
            slideTween = null;
            done?.Invoke();
        }

        void FinishClose(Action done)
        {
            descriptionPaper.SetVisible(false);
            letterPaper.SetVisible(false);
            mode = Mode.None;
            done?.Invoke();
        }

        void ApplySlide(float value)
        {
            slide = value;
            deskRoot.position = deskHome + Vector3.left * (value * viewWidth);
            descriptionPaper.SlideFromRight((1f - value) * viewWidth);
            PlaceLetterPaper();
        }

        void PlaceLetterPaper() => letterPaper.SlideFromRight(((1f - slide) + letterHidden) * viewWidth);

        static Vector2 ViewSize()
        {
            var camera = Camera.main;
            if (!camera || !camera.orthographic) return new Vector2(FallbackViewWidth, FallbackViewHeight);
            return new Vector2(2f * camera.orthographicSize * camera.aspect, 2f * camera.orthographicSize);
        }

        static Vector2 ViewCentre()
        {
            var camera = Camera.main;
            return camera ? (Vector2)camera.transform.position : Vector2.zero;
        }

        void StartListening()
        {
            keyboard = Keyboard.current;
            if (keyboard == null) return;
            listening = true;
            keyboard.onTextInput += OnTextInput;
        }

        void StopListening()
        {
            listening = false;
            if (keyboard != null) keyboard.onTextInput -= OnTextInput;
            keyboard = null;
        }

        void OnTextInput(char character)
        {
            if (!listening || !gate.IsWritable) return;
            if (mode == Mode.Letter) TypeLetter(character);
            else if (mode == Mode.Report) TypeResponse(character);
        }

        void TypeLetter(char character)
        {
            if (!letterTyper.TryType(character)) return;
            sound.PlayKey();
            RenderLetter();
            if (letterTyper.Outcome == LetterOutcome.None) return;
            var outcome = letterTyper.Outcome;
            FinishAfterPause(() => letterFinished?.Invoke(outcome));
        }

        void TypeResponse(char character)
        {
            if (!responseTyper.TryType(character)) return;
            sound.PlayKey();
            RenderResponse();
            if (responseTyper.IsComplete) FinishAfterPause(() => reportFinished?.Invoke());
        }

        void Backspace()
        {
            bool removed = mode == Mode.Letter ? letterTyper.Backspace() : responseTyper.Backspace();
            if (!removed) return;
            sound.PlayBackspace();
            if (mode == Mode.Letter) RenderLetter();
            else RenderResponse();
        }

        void FinishReview()
        {
            StopListening();
            reviewFinished?.Invoke();
        }

        void FinishAfterPause(Action report, float holdFactor = 1f)
        {
            StopListening();
            int token = ++generation;
            DOVirtual.DelayedCall(settings.FinishPauseSeconds * holdFactor, () => ReportIfCurrent(token, report)).SetLink(gameObject);
        }

        void ReportIfCurrent(int token, Action report)
        {
            if (token == generation) report();
        }

        void RevealLeftWithSound()
        {
            ShowLeft(gate.VisibleCount);
            sound.PlayDescriptionKey();
        }

        void ShowLeft(int revealedCount) => descriptionPaper.SetText(leftReveal.Shown(revealedCount), leftReveal.Slots(revealedCount, leftSettledRole, leftRevealedRole), settings.DescriptionTextColor, leftScales);

        void RenderLetter()
        {
            FadeUnchosenAnswer();
            DrawLetter();
        }

        void DrawLetter() => letterPaper.SetText(letterModel.Render(letterTyper.TypedCount, letterTyper.Path, LetterColors(), unchosenAlpha), letterModel.Slots(letterTyper.TypedCount, letterTyper.Path), settings.PendingTextColor);

        void FadeUnchosenAnswer()
        {
            float target = letterTyper.Path == LetterPath.Undecided ? 1f : 0f;
            if (Mathf.Approximately(target, fadeTarget)) return;
            fadeTarget = target;
            fadeTween?.Kill();
            fadeTween = DOVirtual.Float(unchosenAlpha, target, settings.UnchosenFadeSeconds, ApplyUnchosenAlpha).SetLink(gameObject);
        }

        void ApplyUnchosenAlpha(float value)
        {
            unchosenAlpha = value;
            DrawLetter();
        }

        void ResetUnchosenFade()
        {
            fadeTween?.Kill();
            unchosenAlpha = 1f;
            fadeTarget = 1f;
        }

        LetterPalette LetterColors() => new LetterPalette(Hex(settings.TypedTextColor), Hex(settings.PendingTextColor), Hex(settings.AcceptPendingTextColor), Hex(settings.DeclinePendingTextColor));

        void RenderResponse() => letterPaper.SetText(responseModel.RenderTyping(responseTyper.TypedCount, responseTyper.CurrentUnitEnd, Hex(settings.TypedTextColor), Hex(settings.PendingTextColor)), responseModel.TypingSlots(responseTyper.TypedCount, responseTyper.CurrentUnitEnd), settings.PendingTextColor);

        void RenderReview() => letterPaper.SetText(responseModel.RenderReview(Hex(settings.TypedTextColor)), responseModel.ReviewSlots(), settings.TypedTextColor);

        static string Hex(Color color) => ColorUtility.ToHtmlStringRGB(color);
    }
}
