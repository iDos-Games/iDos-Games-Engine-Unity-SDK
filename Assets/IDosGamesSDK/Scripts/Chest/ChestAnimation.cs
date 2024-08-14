using UnityEngine;

namespace IDosGames
{
    [RequireComponent(typeof(Animator))]
    public class ChestAnimation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        [SerializeField] private ParticleSystem _dropEffect;
        [SerializeField] private ParticleSystem _openEffect;
        [SerializeField] private ParticleSystem _showEffect;

        private int _currentState = _animIdle;
        public int ChestState => _currentState;

        private static readonly int _animIdle = Animator.StringToHash("Idle");
        private static readonly int _animOpen = Animator.StringToHash("Open");
        private static readonly int _animClose = Animator.StringToHash("Close");
        private static readonly int _animDisappear = Animator.StringToHash("Disappear");
        private static readonly int _animAppear = Animator.StringToHash("Appear");
        private static readonly int _animDrop = Animator.StringToHash("Drop");
        private static readonly int _animQuickOpenAndClose = Animator.StringToHash("Quick Open and Close");

        public void OnClick()
        {
            Open();
        }

        public void Appear()
        {
            PlayAnimation(_animAppear, 0.5f);
        }

        public void Disappear()
        {
            PlayAnimation(_animDisappear);
        }

        public void Drop()
        {
            PlayAnimation(_animDrop);
        }


        public void Open()
        {
            PlayAnimation(_animOpen);
        }

        private void PlayAnimation(int stringHashState, float transitionDuration = 0.5f)
        {
            if (_currentState == stringHashState)
            {
                return;
            }

            _animator.CrossFade(stringHashState, transitionDuration, 0);
            _currentState = stringHashState;
        }

        public void PlayDropEffect()
        {
            _dropEffect.Play();
        }

        public void PlayOpenEffect()
        {
            _openEffect.Play();
        }

        public void PlayShowEffect()
        {
            _showEffect.Play();
        }
    }
}