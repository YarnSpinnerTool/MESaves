/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity.Attributes;

using TMPro;

#nullable enable

namespace Yarn.Unity.MEImporter
{
    [System.Serializable]
    internal struct InternalAppearance
    {
        [SerializeField] internal Sprite sprite;
        [SerializeField] internal Color colour;
    }

    public sealed class HashtagAwareOptionItem : UnityEngine.UI.Selectable, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler
    {
        [MustNotBeNull][SerializeField] TextMeshProUGUI text;
        [SerializeField] UnityEngine.UI.Image selectionImage;

        [Group("Appearance")][SerializeField] InternalAppearance normal;
        [Group("Appearance")][SerializeField] InternalAppearance selected;
        [Group("Appearance")][SerializeField] InternalAppearance disabled;

        [Group("Appearance")][SerializeField] bool disabledStrikeThrough = true;

        internal InternalAppearance NormalAppearanceForLine
        {
            get
            {
                foreach (var tag in _option.Line.Metadata)
                {
                    if (hashtagStyles.TryGetValue(tag, out var appearance))
                    {
                        return appearance;
                    }
                }
                return normal;
            }
        }

        internal InternalAppearance SelectedAppearanceForLine
        {
            get
            {
                foreach (var tag in _option.Line.Metadata)
                {
                    if (hashtagStyles.TryGetValue(tag, out var appearance))
                    {
                        Color.RGBToHSV(appearance.colour, out var h, out var s, out var v);

                        v += 0.5f;
                        s *= 0.5f;

                        appearance.colour = Color.HSVToRGB(h, s, v);
                        appearance.sprite = selected.sprite;

                        return appearance;
                    }
                }
                return selected;
            }
        }

        public YarnTaskCompletionSource<DialogueOption?>? OnOptionSelected;
        public System.Threading.CancellationToken completionToken;

        [SerializeField] SerializableDictionary<string, InternalAppearance> hashtagStyles = new();

        private bool hasSubmittedOptionSelection = false;

        private DialogueOption _option;
        public DialogueOption Option
        {
            get => _option;

            set
            {
                _option = value;

                hasSubmittedOptionSelection = false;

                // When we're given an Option, use its text and update our
                // interactibility.
                string line = value.Line.TextWithoutCharacterName.Text;
                if (disabledStrikeThrough && !value.IsAvailable)
                {
                    line = $"<s>{value.Line.TextWithoutCharacterName.Text}</s>";
                }
                text.text = line;
                interactable = value.IsAvailable;
                ApplyStyle(NormalAppearanceForLine);
            }
        }

        private void ApplyStyle(InternalAppearance style)
        {
            Color newColour = style.colour;
            Sprite newSprite = style.sprite;
            if (!Option.IsAvailable)
            {
                newColour = disabled.colour;
                newSprite = disabled.sprite;
            }

            text.color = newColour;

            if (selectionImage != null)
            {
                selectionImage.color = newColour;
                if (newSprite != null)
                {
                    selectionImage.sprite = newSprite;
                    selectionImage.gameObject.SetActive(true);
                }
                else
                {
                    selectionImage.gameObject.SetActive(false);
                }
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            ApplyStyle(SelectedAppearanceForLine);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            ApplyStyle(NormalAppearanceForLine);
        }

        new public bool IsHighlighted
        {
            get
            {
                return EventSystem.current.currentSelectedGameObject == this.gameObject;
            }
        }

        // If we receive a submit or click event, invoke our "we just selected this option" handler.
        public void OnSubmit(BaseEventData eventData)
        {
            InvokeOptionSelected();
        }

        public void InvokeOptionSelected()
        {
            // turns out that Selectable subclasses aren't intrinsically interactive/non-interactive
            // based on their canvasgroup, you still need to check at the moment of interaction
            if (!IsInteractable())
            {
                return;
            }

            // We only want to invoke this once, because it's an error to
            // submit an option when the Dialogue Runner isn't expecting it. To
            // prevent this, we'll only invoke this if the flag hasn't been cleared already.
            if (hasSubmittedOptionSelection == false && !completionToken.IsCancellationRequested)
            {
                hasSubmittedOptionSelection = true;
                OnOptionSelected?.TrySetResult(this.Option);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            InvokeOptionSelected();
        }

        // If we mouse-over, we're telling the UI system that this element is
        // the currently 'selected' (i.e. focused) element. 
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.Select();
        }
    }
}
