using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Managers
{
    using System.Collections;
    using Singleton;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using DG.Tweening;
    
    public class PauseManager : SingletonBase<PauseManager>
    {
        private GameObject pauseMenuPanel;
        private GameObject mainMenuContainer;
        private GameObject settingsMenuContainer;
    
        private TMP_Text tutorialToggleText;
        
        private bool isPaused = false;
        private float originalTimeScale = 1f;
    
        public static bool IsPaused => HasInstance && Instance.isPaused;
    
        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
        }
    
        private void Start()
        {
            CreatePauseMenuUIProgrammatically();
        }
    
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    // If StatsPanel is currently active, we close it and reopen the Pause Menu
                    if (UiManager.Instance != null && UiManager.Instance.StatsPanel != null && UiManager.Instance.StatsPanel.activeSelf)
                    {
                        UiManager.Instance.CloseStats();
                        ShowPauseMenuOnly();
                    }
                    else
                    {
                        ResumeGame();
                    }
                }
                else
                {
                    PauseGame();
                }
            }
        }
    
        public void PauseGame()
        {
            if (isPaused) return;
            isPaused = true;
    
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
    
            if (pauseMenuPanel != null)
            {
                // Show main menu container by default
                mainMenuContainer.SetActive(true);
                settingsMenuContainer.SetActive(false);
                
                pauseMenuPanel.SetActive(true);
    
                // CRT flicker animation opening effect
                pauseMenuPanel.transform.localScale = new Vector3(1f, 0.05f, 1f);
                pauseMenuPanel.transform.DOScaleY(1f, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }
    
        public void ResumeGame()
        {
            if (!isPaused) return;
            isPaused = false;
    
            Time.timeScale = originalTimeScale;
    
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.transform.DOScaleY(0.005f, 0.2f).SetEase(Ease.InExpo).SetUpdate(true).OnComplete(() => {
                    pauseMenuPanel.SetActive(false);
                });
            }
        }
    
        private void OpenStatsFromPauseMenu()
        {
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
    
            if (UiManager.Instance != null)
            {
                UiManager.Instance.OpenStats();
            }
        }
    
        public void ShowPauseMenuOnly()
        {
            if (pauseMenuPanel != null)
            {
                mainMenuContainer.SetActive(true);
                settingsMenuContainer.SetActive(false);
                pauseMenuPanel.SetActive(true);
            }
        }
    
        private void ToggleTutorial()
        {
            bool current = PlayerPrefs.GetInt("PlayTutorial", 1) == 1;
            bool next = !current;
            PlayerPrefs.SetInt("PlayTutorial", next ? 1 : 0);
            PlayerPrefs.Save();
    
            // Update TutorialManager if it exists in the scene
            if (TutorialManager.Instance != null)
            {
                if (!next)
                {
                    // If they disabled it midway, complete and shut down the tutorial cleanly
                    TutorialManager.Instance.CompleteTutorial(false);
                }
                else
                {
                    // If they enabled it, restart it immediately
                    TutorialManager.Instance.RestartTutorial();
                }
            }
    
            UpdateTutorialToggleText();
        }
    
        private void UpdateTutorialToggleText()
        {
            if (tutorialToggleText == null) return;
            bool enabled = PlayerPrefs.GetInt("PlayTutorial", 1) == 1;
            tutorialToggleText.text = enabled ? "TUTORIAL: [ENABLED]" : "TUTORIAL: [DISABLED]";
        }
    
        private void OpenSettingsSubmenu()
        {
            mainMenuContainer.SetActive(false);
            settingsMenuContainer.SetActive(true);
            UpdateTutorialToggleText();
        }
    
        private void CloseSettingsSubmenu()
        {
            settingsMenuContainer.SetActive(false);
            mainMenuContainer.SetActive(true);
        }
    
        private void QuitGame()
        {
            Debug.Log("PAUSE_MANAGER: Shutting down game systems...");
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }
    
        private void CreatePauseMenuUIProgrammatically()
        {
            GameObject canvasGo = GameObject.Find("HUD Canvas");
            if (canvasGo == null) canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null) canvasGo = FindFirstObjectByType<Canvas>()?.gameObject;
    
            if (canvasGo == null)
            {
                Debug.LogError("PauseManager: No Canvas found to attach Pause UI!");
                return;
            }
    
            // Main Panel
            pauseMenuPanel = new GameObject("PausePanel_Programmatic", typeof(RectTransform), typeof(Image));
            pauseMenuPanel.transform.SetParent(canvasGo.transform, false);
    
            RectTransform panelRt = pauseMenuPanel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.35f, 0.25f);
            panelRt.anchorMax = new Vector2(0.65f, 0.75f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
    
            Image panelImg = pauseMenuPanel.GetComponent<Image>();
            panelImg.color = new Color(0.01f, 0.05f, 0.01f, 0.96f); // Terminal dark green
    
            Outline outline = pauseMenuPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
            outline.effectDistance = new Vector2(3f, -3f);
    
            // Grid lines overlay
            GameObject gridGo = new GameObject("GridLines", typeof(RectTransform), typeof(Image));
            gridGo.transform.SetParent(pauseMenuPanel.transform, false);
            RectTransform gridRt = gridGo.GetComponent<RectTransform>();
            gridRt.anchorMin = Vector2.zero;
            gridRt.anchorMax = Vector2.one;
            gridRt.offsetMin = Vector2.zero;
            gridRt.offsetMax = Vector2.zero;
            Image gridImg = gridGo.GetComponent<Image>();
            gridImg.color = new Color(0.1f, 0.25f, 0.1f, 0.05f);
            gridImg.raycastTarget = false;
    
            // Scanline overlay
            GameObject scanlineGo = new GameObject("Scanline", typeof(RectTransform), typeof(Image));
            scanlineGo.transform.SetParent(pauseMenuPanel.transform, false);
            RectTransform scanlineRt = scanlineGo.GetComponent<RectTransform>();
            scanlineRt.anchorMin = new Vector2(0f, 0.96f);
            scanlineRt.anchorMax = new Vector2(1f, 1f);
            scanlineRt.offsetMin = Vector2.zero;
            scanlineRt.offsetMax = Vector2.zero;
            Image scanlineImg = scanlineGo.GetComponent<Image>();
            scanlineImg.color = new Color(0.2f, 0.9f, 0.2f, 0.08f);
            scanlineImg.raycastTarget = false;
    
            // Scanline animation
            scanlineRt.anchorMin = new Vector2(0f, 1f);
            scanlineRt.anchorMax = new Vector2(1f, 1.05f);
            scanlineRt.DOAnchorMin(new Vector2(0f, -0.05f), 4f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetUpdate(true);
            scanlineRt.DOAnchorMax(new Vector2(1f, -0.01f), 4f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetUpdate(true);
    
            // Header Title
            GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(pauseMenuPanel.transform, false);
            RectTransform titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.82f);
            titleRt.anchorMax = new Vector2(1f, 0.96f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;
    
            TextMeshProUGUI titleTxt = titleGo.GetComponent<TextMeshProUGUI>();
            titleTxt.text = "SYSTEM_PAUSED // TERMINAL";
            titleTxt.fontSize = 32;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = new Color(0.3f, 1f, 0.3f, 1f);
            titleTxt.alignment = TextAlignmentOptions.Center;
    
            // Divider
            GameObject divGo = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            divGo.transform.SetParent(pauseMenuPanel.transform, false);
            RectTransform divRt = divGo.GetComponent<RectTransform>();
            divRt.anchorMin = new Vector2(0.1f, 0.81f);
            divRt.anchorMax = new Vector2(0.9f, 0.82f);
            divRt.offsetMin = Vector2.zero;
            divRt.offsetMax = Vector2.zero;
            divGo.GetComponent<Image>().color = new Color(0.2f, 0.9f, 0.2f, 0.5f);
    
            // ------------------ MAIN MENU PANEL ------------------
            mainMenuContainer = new GameObject("MainMenuContainer", typeof(RectTransform));
            mainMenuContainer.transform.SetParent(pauseMenuPanel.transform, false);
            RectTransform mmRt = mainMenuContainer.GetComponent<RectTransform>();
            mmRt.anchorMin = new Vector2(0f, 0f);
            mmRt.anchorMax = new Vector2(1f, 0.8f);
            mmRt.offsetMin = Vector2.zero;
            mmRt.offsetMax = Vector2.zero;
    
            // Button 1: Resume
            CreateMenuButton("ResumeBtn", mmRt, "RESUME GAME", 180f, () => ResumeGame());
    
            // Button 2: Stats
            CreateMenuButton("StatsBtn", mmRt, "VIEW STATS", 80f, () => OpenStatsFromPauseMenu());
    
            // Button 3: Settings
            CreateMenuButton("SettingsBtn", mmRt, "SETTINGS MENU", -20f, () => OpenSettingsSubmenu());
    
            // Button 4: Quit
            CreateMenuButton("QuitBtn", mmRt, "ABORT RUN [QUIT]", -120f, () => QuitGame());
    
            // ------------------ SETTINGS SUB-MENU ------------------
            settingsMenuContainer = new GameObject("SettingsMenuContainer", typeof(RectTransform));
            settingsMenuContainer.transform.SetParent(pauseMenuPanel.transform, false);
            RectTransform smRt = settingsMenuContainer.GetComponent<RectTransform>();
            smRt.anchorMin = new Vector2(0f, 0f);
            smRt.anchorMax = new Vector2(1f, 0.8f);
            smRt.offsetMin = Vector2.zero;
            smRt.offsetMax = Vector2.zero;
    
            // Button 1: Toggle Tutorial
            GameObject toggleBtn = CreateMenuButton("ToggleTutorialBtn", smRt, "TUTORIAL: [ENABLED]", 120f, () => ToggleTutorial());
            tutorialToggleText = toggleBtn.GetComponentInChildren<TextMeshProUGUI>();
    
            // Button 2: Back
            CreateMenuButton("BackBtn", smRt, "RETURN TO MENU", 0f, () => CloseSettingsSubmenu());
    
            settingsMenuContainer.SetActive(false);
            pauseMenuPanel.SetActive(false);
        }
    
        private GameObject CreateMenuButton(string name, RectTransform parent, string labelText, float posY, System.Action onClickAction)
        {
            GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);
    
            RectTransform rt = btnGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.15f, 0.5f);
            rt.anchorMax = new Vector2(0.85f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, posY);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, 70f);
    
            Image img = btnGo.GetComponent<Image>();
            img.color = new Color(0.05f, 0.15f, 0.05f, 0.85f);
    
            Outline outline = btnGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.8f, 0.1f, 0.5f);
            outline.effectDistance = new Vector2(2f, -2f);
    
            GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
    
            TextMeshProUGUI txt = textGo.GetComponent<TextMeshProUGUI>();
            txt.text = labelText;
            txt.fontSize = 26;
            txt.fontStyle = FontStyles.Bold;
            txt.color = new Color(0.2f, 0.9f, 0.2f, 1f);
            txt.alignment = TextAlignmentOptions.Center;
    
            Button btn = btnGo.GetComponent<Button>();
            btn.onClick.AddListener(() => onClickAction());
    
            AddHoverAnimations(btn, txt, labelText);
    
            return btnGo;
        }
    
        private void AddHoverAnimations(Button btn, TextMeshProUGUI txt, string origText)
        {
            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>() ?? btn.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();
    
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => {
                btn.transform.DOScale(1.05f, 0.12f).SetUpdate(true);
                txt.text = "> " + txt.text.Replace("> ", "").Replace(" <", "") + " <";
                txt.color = new Color(0.5f, 1f, 0.5f, 1f);
            });
            trigger.triggers.Add(entryEnter);
    
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) => {
                btn.transform.DOScale(1f, 0.12f).SetUpdate(true);
                
                // Re-fetch custom dynamic label if it's the toggle button, otherwise restore origText
                string cleanText = origText;
                if (btn.gameObject.name == "ToggleTutorialBtn" && tutorialToggleText != null)
                {
                    cleanText = tutorialToggleText.text;
                }
                cleanText = cleanText.Replace("> ", "").Replace(" <", "");
                
                txt.text = cleanText;
                txt.color = new Color(0.2f, 0.9f, 0.2f, 1f);
            });
            trigger.triggers.Add(entryExit);
        }
    }
    
}


