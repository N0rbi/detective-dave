using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class BriefingView : MonoBehaviour, IBriefingScreen
    {
        const float FallbackCameraWidth = 20f;
        const float DescriptionLeft = 0.05f;
        const float DescriptionTop = 0.1f;
        const float DescriptionWidth = 0.4f;
        const float DescriptionHeight = 0.8f;
        const float PaperLeft = 0.55f;
        const float PaperTop = 0.06f;
        const float PaperWidth = 0.4f;
        const float PaperHeight = 0.88f;
        const float PaperPadding = 0.04f;

        OverworldSettings settings;
        Func<string, CaseFile> findCase;
        Transform deskRoot;
        TypewriterSound sound;
        Tween slideTween;
        Keyboard keyboard;
        DivergingTyper typer;
        LetterPaperModel paper;
        TimedReveal reveal;
        Action<LetterOutcome> finished;
        GUIStyle descriptionStyle;
        GUIStyle letterStyle;
        Vector3 deskHome;
        string description = string.Empty;
        string letter = string.Empty;
        float slide;
        bool typing;
        int generation;

        public void Configure(OverworldSettings overworldSettings, Func<string, CaseFile> caseLookup, Transform slidingDesk, TypewriterSound typewriterSound)
        {
            settings = overworldSettings;
            findCase = caseLookup;
            deskRoot = slidingDesk;
            sound = typewriterSound;
        }

        public void Open(string caseId, Action<LetterOutcome> finishedCallback)
        {
            finished = finishedCallback;
            PrepareTexts(findCase(caseId));
            deskHome = deskRoot.position;
            SlideTo(1f, StartTyping);
        }

        public void Close(Action done)
        {
            StopTyping();
            generation++;
            SlideTo(0f, done);
        }

        void Update()
        {
            if (!typing) return;
            reveal.Advance(Time.deltaTime);
            if (keyboard != null && keyboard.backspaceKey.wasPressedThisFrame) Backspace();
        }

        void OnDisable()
        {
            StopTyping();
            slideTween?.Kill();
        }

        void OnGUI()
        {
            if (slide <= 0f || Event.current.type != EventType.Repaint) return;
            EnsureStyles();
            float width = Screen.width;
            float height = Screen.height;
            float offset = (1f - slide) * width;
            Fill(new Rect(offset, 0f, width, height), settings.BriefingBackdropColor);
            GUI.Label(new Rect(offset + width * DescriptionLeft, height * DescriptionTop, width * DescriptionWidth, height * DescriptionHeight), description.Substring(0, reveal.VisibleCount), descriptionStyle);
            var paperRect = new Rect(offset + width * PaperLeft, height * PaperTop, width * PaperWidth, height * PaperHeight);
            Fill(paperRect, settings.PaperColor);
            GUI.Label(Padded(paperRect), letter, letterStyle);
        }

        void PrepareTexts(CaseFile file)
        {
            typer = new DivergingTyper(file.ResolvedAcceptLetter, file.ResolvedDeclineLetter, settings.TypingRules);
            paper = new LetterPaperModel(file.ClientName, file.ResolvedAcceptMessage, file.ResolvedDeclineMessage, int.MaxValue);
            description = file.ResolvedDescription;
            reveal = new TimedReveal(description.Length, settings.DescriptionCharactersPerSecond);
            letter = RenderLetter();
        }

        string RenderLetter() => paper.Render(typer.TypedCount, typer.Path, Hex(settings.TypedTextColor), Hex(settings.PendingTextColor));

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

        void ApplySlide(float value)
        {
            slide = value;
            deskRoot.position = deskHome + Vector3.left * (value * CameraWidth());
        }

        static float CameraWidth()
        {
            var camera = Camera.main;
            return camera && camera.orthographic ? 2f * camera.orthographicSize * camera.aspect : FallbackCameraWidth;
        }

        void StartTyping()
        {
            keyboard = Keyboard.current;
            if (keyboard == null) return;
            typing = true;
            keyboard.onTextInput += OnTextInput;
        }

        void StopTyping()
        {
            typing = false;
            if (keyboard != null) keyboard.onTextInput -= OnTextInput;
            keyboard = null;
        }

        void OnTextInput(char character)
        {
            if (!typing || !typer.TryType(character)) return;
            sound.Play();
            letter = RenderLetter();
            FinishWhenComplete();
        }

        void Backspace()
        {
            if (typer.Backspace()) letter = RenderLetter();
        }

        void FinishWhenComplete()
        {
            var outcome = typer.Outcome;
            if (outcome == LetterOutcome.None) return;
            StopTyping();
            int token = ++generation;
            DOVirtual.DelayedCall(settings.FinishPauseSeconds, () => ReportOutcome(token, outcome)).SetLink(gameObject);
        }

        void ReportOutcome(int token, LetterOutcome outcome)
        {
            if (token == generation) finished?.Invoke(outcome);
        }

        void EnsureStyles()
        {
            int fontSize = Mathf.RoundToInt(Screen.height * settings.TextHeightShare);
            descriptionStyle = descriptionStyle ?? NewStyle();
            letterStyle = letterStyle ?? NewStyle();
            descriptionStyle.fontSize = fontSize;
            descriptionStyle.normal.textColor = settings.DescriptionColor;
            letterStyle.fontSize = fontSize;
            letterStyle.normal.textColor = settings.PendingTextColor;
        }

        static GUIStyle NewStyle() => new GUIStyle(GUI.skin.label) { wordWrap = true, richText = true, alignment = TextAnchor.UpperLeft };

        static void Fill(Rect area, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        static Rect Padded(Rect area)
        {
            float padding = area.width * PaperPadding;
            return new Rect(area.x + padding, area.y + padding, area.width - padding * 2f, area.height - padding * 2f);
        }

        static string Hex(Color color) => ColorUtility.ToHtmlStringRGB(color);
    }
}
