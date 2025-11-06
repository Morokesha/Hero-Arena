using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Extension;
using Core.Heroes;
using Core.Heroes.Skills;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

namespace Core.Battle
{
    public class TargetSelector : MonoBehaviour
    {
        [Header("Настройки")]
        public Camera mainCamera;
        
        public float cameraZoomDistance = 3f; // Расстояние перед лицом героя
        public float cameraZoomHeight = 1.5f; // Высота камеры над головой героя
        public float cameraMoveSpeed = 5f;

        [Header("Прицел")]
        public GameObject targetReticlePrefab; // Префаб прицела (кольцо под ногами)
        private GameObject _currentReticle;
        public Color allyReticleColor = Color.green; // Зеленый для союзников (Player)
        public Color enemyReticleColor = Color.red; // Красный для врагов (AI)

        [Header("Курсор для удержания")]
        public Canvas cursorCanvas; // Canvas для курсора (добавь в сцену, Render Mode: Screen Space - Overlay)

        public Image cursorFillImage; // Image с типом Filled (Radial 360), начальный fillAmount = 0
        public float holdTime = 2f; // Время удержания для открытия меню
        public float holdThreshold = 50f; // Порог расстояния (в пикселях) для сброса удержания при смещении курсора

        private Hero _selectedTarget;
        private Vector3 _cameraOriginalPosition;
        private Quaternion _cameraOriginalRotation;
        private bool _isZoomed = false;

        // Таймеры и флаги для удержания
        private float _holdTimer = 0f;
        private bool _isHolding = false;
        private Hero _holdCandidate = null;
        private Vector2 _holdStartPos; // Позиция начала удержания
        private bool _menuOpened = false; // Флаг, чтобы меню не открывалось многократно

        // Кэшированные ссылки для оптимизации (избежать аллокаций в Update)
        private RectTransform _canvasRect; // Кэшируем RectTransform Canvas
        private Camera _canvasCamera; // Кэшируем камеру Canvas
        
        public event Action<Hero, bool> TargetSelected; // target, isAlly (true для союзников, false для врагов)
        public event Action<Hero> OnHeroMenuOpened; // Для открытия меню героя

        void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
            {
                _cameraOriginalPosition = mainCamera.transform.position;
                _cameraOriginalRotation = mainCamera.transform.rotation;
            }

            // Кэшируем для оптимизации
            if (cursorCanvas != null)
            {
                _canvasRect = cursorCanvas.GetComponent<RectTransform>();
                _canvasCamera = cursorCanvas.worldCamera;
            }

            // Инициализация курсора
            if (cursorFillImage != null)
                cursorFillImage.fillAmount = 0f;
        }

        void Update()
        {
            HandleMouseInput();

            if (_isZoomed && _selectedTarget != null && !_isHolding) // Не двигать камеру во время удержания
                SmoothMoveCameraToTarget();

            // Обновляем позицию курсора только если удерживаем (оптимизация)
            if (_isHolding) 
                UpdateCursorPosition();
        }

        private void HandleMouseInput()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _holdStartPos = Mouse.current.position.ReadValue(); // Запомнить позицию начала
                TrySelectTarget();
                _holdTimer = 0f;
                _isHolding = true;
                _menuOpened = false;
                _holdCandidate = _selectedTarget; // Кандидат для удержания
                UpdateCursorPosition(); // Обновить позицию сразу при старте удержания
            }

            if (Mouse.current.leftButton.isPressed && _isHolding)
            {
                // Проверить расстояние от места зажатия
                if (Vector2.Distance(Mouse.current.position.ReadValue(), _holdStartPos) > holdThreshold)
                {
                    // Сбросить удержание, если курсор ушёл далеко
                    _isHolding = false;
                    _holdTimer = 0f;
                    UpdateCursorFill(0f);
                    _holdCandidate = null;
                    return;
                }

                _holdTimer += Time.deltaTime;
                UpdateCursorFill(_holdTimer / holdTime);

                if (_holdTimer >= holdTime && _holdCandidate != null && !_menuOpened)
                {
                    OpenHeroMenu(_holdCandidate);
                    _menuOpened = true;
                    _isHolding = false; // Остановить удержание после открытия меню
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isHolding = false;
                _holdTimer = 0f;
                UpdateCursorFill(0f);
                _holdCandidate = null;
            }
        }

        private void TrySelectTarget()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()); // Новый API для позиции мыши
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Hero hero = hit.collider.GetComponentInParent<Hero>();
                if (hero != null)
                {
                    SelectTarget(hero);
                }
            }
        }

        private void SelectTarget(Hero hero)
        {
            ResetSelection();

            _selectedTarget = hero;

            _currentReticle = Instantiate(targetReticlePrefab);
            _currentReticle.transform.position = GetReticlePosition(_selectedTarget);

            Image reticleRenderer = _currentReticle.GetComponentInChildren<Image>();
            if (reticleRenderer != null)
            {
                reticleRenderer.color = (hero.GetHeroTeam() == HeroTeam.Player) ? allyReticleColor : enemyReticleColor;
            }

            bool isAlly = (hero.GetHeroTeam() == HeroTeam.Player);
            TargetSelected?.Invoke(_selectedTarget, isAlly);
        }

        private Vector3 GetReticlePosition(Hero hero) =>
            new(hero.transform.position.x, hero.transform.position.y + 0.05f, hero.transform.position.z);

        private void SmoothMoveCameraToTarget()
        {
            Vector3 targetPos = _selectedTarget.transform.position + _selectedTarget.transform.forward *
                cameraZoomDistance + Vector3.up * cameraZoomHeight;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime *
                cameraMoveSpeed);

            // Камера смотрит на героя
            Quaternion lookRotation = Quaternion.LookRotation(_selectedTarget.transform.position -
                                                              mainCamera.transform.position);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, lookRotation,
                Time.deltaTime * cameraMoveSpeed);

            if (Vector3.Distance(mainCamera.transform.position, targetPos) < 0.1f)
            {
                // Камера на месте
            }

            if (_currentReticle != null)
                _currentReticle.transform.position = GetReticlePosition(_selectedTarget);
        }

        private void UpdateCursorFill(float fillAmount)
        {
            if (cursorFillImage != null)
                cursorFillImage.fillAmount = Mathf.Clamp01(fillAmount);
        }

        private void UpdateCursorPosition()
        {
            if (_canvasRect != null && cursorFillImage != null && _canvasCamera != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _canvasRect, mousePos, _canvasCamera, out Vector2 localPoint))
                {
                    cursorFillImage.rectTransform.localPosition = localPoint;
                }
            }
        }

        private void OpenHeroMenu(Hero hero)
        {
            Debug.Log($"Открываем меню для героя: {hero.name} (Team: {hero.GetHeroTeam()})");
            OnHeroMenuOpened?.Invoke(hero);

            // Сбросить выбор (прицел, target) при открытии меню
            ResetSelection();

            // Зум только при открытии меню
            _isZoomed = true;
            // Здесь позже добавь логику открытия UI
        }

        private void ResetSelection()
        {
            _selectedTarget = null;

            if (_currentReticle != null)
            {
                Destroy(_currentReticle);
                _currentReticle = null;
            }
        }

        public void ResetZoom()
        {
            _isZoomed = false;
            StartCoroutine(MoveCameraBack());
        }

        private IEnumerator MoveCameraBack()
        {
            while (Vector3.Distance(mainCamera.transform.position, _cameraOriginalPosition) > 0.05f)
            {
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, _cameraOriginalPosition,
                    Time.deltaTime * cameraMoveSpeed);
                mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, _cameraOriginalRotation,
                    Time.deltaTime * cameraMoveSpeed);
                yield return null;
            }

            _isZoomed = false;
        }

        public Hero GetSelectedTarget()
        {
            return _selectedTarget;
        }
    }
}