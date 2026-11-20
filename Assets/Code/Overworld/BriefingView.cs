using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class BriefingView : MonoBehaviour, IBriefingScreen
    {
        const float FallbackViewWidth = 20f;
        const float FallbackViewHeight = 11.25f;

        enum Mode
        {
            None,
            Letter,
            Findings
        }

        OverworldSettings settings;
        Func<string, CaseFile> findCase;
        Transform deskRoot;
        TypewriterSound sound;
        PaperView descriptionPaper;
        PaperView letterPaper;
        Tween slideTween;
        Tween appearTween;
        Keyboard keyboard;
        BriefingGate gate;
        DivergingTyper letterTyper;
        LetterPaperModel letterModel;
        StoryTyper storyTyper;
        FindingsPaperModel findingsModel;
        Action<LetterOutcome> letterFinished;
        Action findingsFinished;
        Mode mode;
        Vector3 deskHome;
        string description = string.Empty;
        float slide;
        float letterHidden = 1f;
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
            var letters = PlacePapers();
            description = TextWrap.Wrap(file.ResolvedDescription, letters.Description);
            letterTyper = new DivergingTyper(file.ResolvedAcceptLetter, file.ResolvedDeclineLetter, settings.TypingRules);
            letterModel = new LetterPaperModel(file.ClientName, file.ResolvedAcceptMessage, file.ResolvedDeclineMessage, letters.Letter);
            gate = new BriefingGate(new TimedReveal(description.Length, settings.DescriptionSecondsPerCharacter, settings.DescriptionSkipSpeedMultiplier));
            ShowDescription(0);
            RenderLetter();
            Begin();
        }

        public void OpenFindings(string caseId, Action finishedCallback)
        {
            var file = findCase(caseId);
            mode = Mode.Findings;
            findingsFinished = finishedCallback;
            var letters = PlacePapers();
            description = TextWrap.Wrap(file.ResolvedDescription, letters.Description);
            storyTyper = new StoryTyper(file.ResolvedFindings, settings.TypingRules, settings.FindingsUnit);
            findingsModel = new FindingsPaperModel(storyTyper.Text, letters.Letter);
            gate = new BriefingGate(new TimedReveal(0, settings.DescriptionSecondsPerCharacter, settings.DescriptionSkipSpeedMultiplier));
            ShowDescription(description.Length);
            RenderFindings();
            Begin();
        }

        public void Close(Action done)
        {
            StopListening();
            generation++;
            appearTween?.Kill();
            SlideTo(0f, () => FinishClose(done));
        }

        void Update()
        {
            if (!listening) return;
            if (keyboard.anyKey.wasPressedThisFrame) gate.OnAnyKey();
            bool wasWritable = gate.IsWritable;
            int revealed = gate.Tick(Time.deltaTime);
            if (revealed > 0 && mode == Mode.Letter) RevealDescription();
            if (!wasWritable && gate.IsWritable) ShowLetterPaper();
            if (gate.IsWritable && keyboard.backspaceKey.wasPressedThisFrame) Backspace();
        }

        void OnDisable()
        {
            StopListening();
            slideTween?.Kill();
            appearTween?.Kill();
        }

        PaperView NewPaper(string paperName, Sprite sprite)
        {
            var paper = new GameObject(paperName).AddComponent<PaperView>();
            paper.transform.SetParent(transform, false);
            paper.Configure(sprite, settings.TextFont);
            return paper;
        }

        (int Description, int Letter) PlacePapers()
        {
            var view = ViewSize();
            var centre = ViewCentre();
            viewWidth = view.x;
            float lineHeight = view.y * settings.TextHeightShare;
            var descriptionSpot = BriefingLayout.Place(settings.DescriptionPlacement, centre, view);
            var letterSpot = BriefingLayout.Place(settings.LetterPlacement, centre, view);
            descriptionPaper.Place(descriptionSpot, lineHeight, settings.PaperPaddingShare);
            letterPaper.Place(letterSpot, lineHeight, settings.PaperPaddingShare);
            return (LettersPerLine(descriptionSpot, lineHeight), LettersPerLine(letterSpot, lineHeight));
        }

        int LettersPerLine(PaperSpot spot, float lineHeight) => BriefingLayout.LettersPerLine(spot.Width, settings.PaperPaddingShare, lineHeight, settings.TextAdvanceRatio);

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
            else if (mode == Mode.Findings) TypeFindings(character);
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

        void TypeFindings(char character)
        {
            if (!storyTyper.TryType(character)) return;
            sound.PlayKey();
            RenderFindings();
            if (storyTyper.IsComplete) FinishAfterPause(() => findingsFinished?.Invoke());
        }

        void Backspace()
        {
            bool removed = mode == Mode.Letter ? letterTyper.Backspace() : storyTyper.Backspace();
            if (!removed) return;
            sound.PlayBackspace();
            if (mode == Mode.Letter) RenderLetter();
            else RenderFindings();
        }

        void FinishAfterPause(Action report)
        {
            StopListening();
            int token = ++generation;
            DOVirtual.DelayedCall(settings.FinishPauseSeconds, () => ReportIfCurrent(token, report)).SetLink(gameObject);
        }

        void ReportIfCurrent(int token, Action report)
        {
            if (token == generation) report();
        }

        void RevealDescription() => ShowDescription(gate.VisibleCount);

        void ShowDescription(int visibleCount) => descriptionPaper.SetText(description.Substring(0, Mathf.Min(visibleCount, description.Length)), settings.DescriptionTextColor);

        void RenderLetter() => letterPaper.SetText(letterModel.Render(letterTyper.TypedCount, letterTyper.Path, Hex(settings.TypedTextColor), Hex(settings.PendingTextColor)), settings.PendingTextColor);

        void RenderFindings() => letterPaper.SetText(findingsModel.Render(storyTyper.RevealedCount, Hex(settings.TypedTextColor), Hex(settings.PendingTextColor)), settings.PendingTextColor);

        static string Hex(Color color) => ColorUtility.ToHtmlStringRGB(color);
    }
}
