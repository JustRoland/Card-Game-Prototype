using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Characters;
using Characters.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

namespace Cards
{
    public class Hand : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField]
        private InterfaceReference<IStats> statsHolder;

        [FoldoutGroup("Hand Settings"), SerializeField]
        private SplineContainer splineContainer;

        [FoldoutGroup("Hand Settings"), SerializeField]
        private float cardSpacing = 0.1f;

        [FoldoutGroup("Hand Settings"), SerializeField]
        private bool dynamicCardSpacing;

        [FoldoutGroup("Hand Settings"), SerializeField]
        private float cardPositionUpdateTime = 0.15f;

        [FoldoutGroup("Hand Settings"), SerializeField]
        private float dealSpeed = 0.1f;
        
        [FoldoutGroup("Hand Settings"), SerializeField]
        private float cardScaleUpTime = 0.15f;

        [FoldoutGroup("Hand Settings"), SerializeField]
        private int maxCards = 8;

        [ReadOnly] [ShowInInspector] private int CurrentCards => _cards.Count;

        [FoldoutGroup("Hand Settings"), SerializeField]
        private int startingHand = 5;

        [FoldoutGroup("Hand Settings"), SerializeField]
        private Vector3 hoverCardPositionOffset = Vector3.zero;

        private float _normalHandYPosition;

        [FoldoutGroup("Hand Settings"), SerializeField]
        private float hideHandYPosition = -5f;

        [FoldoutGroup("Hand Settings"), SerializeField]
        private float hideHandMoveTime = 1f;

        [FoldoutGroup("Events")] public UnityEvent<Hand> RequestCardView = new();
        [FoldoutGroup("Events")] public UnityEvent<Hand, CardView> RequestRemoveCardView = new();

        private readonly List<CardView> _cards = new();

        public CardHoverSystem HoverSystem { get; private set; }


        [Header("Temporary For Testing")] [SerializeField]
        private InputActionReference inputAdd;

        [SerializeField] private InputActionReference inputRemove;
        [SerializeField] private InputActionReference inputHide;
        
        private CancellationTokenSource _cardPositionUpdateTokenSource;
        
        public bool Hide { get; private set; }

        private void Awake()
        {
            HoverSystem = GetComponent<CardHoverSystem>();
            HoverSystem.Initialize(hoverCardPositionOffset);
        }

        private void Start()
        {
            _cardPositionUpdateTokenSource = new CancellationTokenSource();
            _normalHandYPosition = transform.localPosition.y;
            DealCards(startingHand, dealSpeed, .5f).Forget();
        }


        private async UniTask DealCards(int amount, float speed, float withDelay = 0, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) return;
            await UniTask.WaitForSeconds(withDelay, cancellationToken: cancellationToken);
            for (int i = 0; i < amount; i++)
            {
                RequestCardView.Invoke(this);
                await UniTask.WaitForSeconds(speed, cancellationToken: cancellationToken);
            }

            await UniTask.Yield();
        }


        public void UpdateInput(CharacterInput input)
        {
            if (input.ToggleCards) ToggleHand();
        }

        //TEMPORARY
        private void Update()
        {
            if (inputAdd.action.WasPressedThisFrame()) AddCard();

            if (inputRemove.action.WasPressedThisFrame()) RemoveCard();
        }

        [PropertySpace, ButtonGroup, GUIColor(0.5f, 1, 0.4f)]
        private void AddCard()
        {
            if (_cards.Count >= maxCards) return;
            RequestCardView.Invoke(this);
        }

        [PropertySpace, ButtonGroup, GUIColor(1f, .1f, .2f)]
        private void RemoveCard()
        {
            if (_cards.Count <= 0) return;
            var card = _cards.FirstOrDefault();
            RequestRemoveCardView.Invoke(this, card);
        }

        [PropertySpace, ButtonGroup]
        private void ToggleHand()
        {
            Hide = !Hide;
            transform.DOLocalMoveY(Hide ? hideHandYPosition : _normalHandYPosition, hideHandMoveTime);
            Cursor.lockState = Hide ? CursorLockMode.Locked : CursorLockMode.Confined;
            Cursor.visible = !Hide;
            _cards.Where(c => c.Selected).ToList().ForEach(c =>
            {
                c.SelectCard(false, false);
                CardManager.Instance.SelectedCards.Remove(c);
            });
            GameManager.Instance?.SlowTime(Hide ? Speed.Normal : Speed.Slow);
        }

        public void AddCard(CardView cardView)
        {
            if (!cardView) return;

            cardView.transform.parent = transform;
            _cards.Add(cardView);

            if (statsHolder.Value != null) {
                //Handle effects
                for (var i = 0; i < cardView.Card.PassiveEffects.Length; i++)
                {
                    var effect = cardView.Card.PassiveEffects[i];
                    var modifier = new BasicModifier(effect.statType, effect.duration, v => v + effect.value);
                    statsHolder.Value.Stats.Mediator.AddModifiers(modifier);
                    cardView.Card.modifiersID[i] = modifier.ID;
                    var colorModifier = new ColorModifier(effect.category, effect.color, effect.duration);
                    statsHolder.Value.Stats.Mediator.AddModifiers(colorModifier);
                    if (effect.trait is not TraitType.None) statsHolder.Value.Stats.UnlockTrait(effect.trait);
                }
            }


            _cardPositionUpdateTokenSource.Cancel();
            _cardPositionUpdateTokenSource.Dispose();
            _cardPositionUpdateTokenSource = new CancellationTokenSource();
            UpdateCardPositions(cardPositionUpdateTime, _cardPositionUpdateTokenSource.Token).Forget();

        }

        public void RemoveCard(CardView[] cardViews)
        {
            if (cardViews == null) return;

            foreach (var cardView in cardViews)
            {
                if (!_cards.Contains(cardView)) continue;
                _cards.Remove(cardView);
                if (statsHolder.Value == null) continue;
                foreach (var id in cardView.Card.modifiersID)
                {
                    statsHolder.Value.Stats.Mediator.RemoveModifier(id);
                }
            }

            _cardPositionUpdateTokenSource.Cancel();
            _cardPositionUpdateTokenSource.Dispose();
            _cardPositionUpdateTokenSource = new CancellationTokenSource();
            UpdateCardPositions(cardPositionUpdateTime, _cardPositionUpdateTokenSource.Token).Forget();
        }

        private async UniTask UpdateCardPositions(float seconds, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            if (_cards.Count == 0) return;
            var spacing = dynamicCardSpacing ? Math.Min(1f / _cards.Count, cardSpacing) : cardSpacing;
            float firstCardPosition = 0.5f - (_cards.Count - 1) * spacing / 2;
            Spline spline = splineContainer.Spline;
            for (int i = 0; i < _cards.Count; i++)
            {
                var card = _cards[i];
                float p = firstCardPosition + i * spacing;
                Vector3 splinePos = spline.EvaluatePosition(p);
                Vector3 forward = spline.EvaluateTangent(p);
                Vector3 up = spline.EvaluateUpVector(p);
                Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
                card.transform.DOKill();
                if (card.transform.localScale == Vector3.zero) card.transform.DOScale(transform.localScale, cardScaleUpTime);
                card.transform.DOLocalMove(splinePos * transform.localScale.x + new Vector3(0, 0, -0.1f * i), seconds);
                card.transform.DOLocalRotate(rotation.eulerAngles, seconds);
                card.Order = i;
            }

            await UniTask.WaitForSeconds(seconds, cancellationToken: cancellationToken);
        }
    }
}