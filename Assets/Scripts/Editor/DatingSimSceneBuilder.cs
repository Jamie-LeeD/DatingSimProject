using DatingSim.Characters;
using DatingSim.Core;
using DatingSim.Dialogue;
using DatingSim.SaveSystem;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DatingSim.EditorTools
{
    public static class DatingSimSceneBuilder
    {
        [MenuItem("Tools/Create Dating Sim Scene")]
        public static void CreateDatingSimScene()
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Create Dating Sim Scene");

            Canvas canvas = EnsureCanvas();
            EnsureEventSystem();

            GameObject gameManager = EnsureGameManager(out DialogueManager dialogueManager, out TypewriterEffect typewriterEffect, out RelationshipManager relationshipManager, out SaveSystemManager saveSystemManager, out AudioManager audioManager);
            BuildGameManagerReferences(gameManager, dialogueManager, typewriterEffect, relationshipManager, saveSystemManager, audioManager);

            BuildSceneUi(canvas.transform, dialogueManager, typewriterEffect, saveSystemManager, audioManager);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = gameManager;
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        private static Canvas EnsureCanvas()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
                canvas = canvasGo.GetComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystemGo, "Create EventSystem");
        }

        private static GameObject EnsureGameManager(
            out DialogueManager dialogueManager,
            out TypewriterEffect typewriterEffect,
            out RelationshipManager relationshipManager,
            out SaveSystemManager saveSystemManager,
            out AudioManager audioManager)
        {
            GameObject gameManager = GetOrCreateRoot("GameManager");

            EnsureComponent<GameInitializer>(gameManager);
            dialogueManager = EnsureComponent<DialogueManager>(gameManager);
            typewriterEffect = EnsureComponent<TypewriterEffect>(gameManager);
            relationshipManager = EnsureComponent<RelationshipManager>(gameManager);
            saveSystemManager = EnsureComponent<SaveSystemManager>(gameManager);
            EnsureComponent<GameFlowStateMachine>(gameManager);
            EnsureComponent<DialogueGameplayConnector>(gameManager);

            GameObject audioManagerGo = GetOrCreateChild(gameManager, "AudioManager");
            audioManager = EnsureComponent<AudioManager>(audioManagerGo);

            AudioSource bgmSource = GetOrCreateNamedAudioSource(audioManagerGo, "BGM Source", true);
            AudioSource sfxSource = GetOrCreateNamedAudioSource(audioManagerGo, "SFX Source", false);

            SerializedObject audioSerialized = new SerializedObject(audioManager);
            audioSerialized.FindProperty("bgmSource").objectReferenceValue = bgmSource;
            audioSerialized.FindProperty("sfxSource").objectReferenceValue = sfxSource;
            audioSerialized.ApplyModifiedPropertiesWithoutUndo();

            return gameManager;
        }

        private static void BuildGameManagerReferences(
            GameObject gameManager,
            DialogueManager dialogueManager,
            TypewriterEffect typewriterEffect,
            RelationshipManager relationshipManager,
            SaveSystemManager saveSystemManager,
            AudioManager audioManager)
        {
            GameInitializer initializer = gameManager.GetComponent<GameInitializer>();
            SerializedObject initSerialized = new SerializedObject(initializer);
            SetIfExists(initSerialized, "dialogueManager", dialogueManager);
            SetIfExists(initSerialized, "relationshipManager", relationshipManager);
            SetIfExists(initSerialized, "saveSystemManager", saveSystemManager);
            SetIfExists(initSerialized, "audioManager", audioManager);
            SetIfExists(initSerialized, "initializeOnAwake", true);
            SetIfExists(initSerialized, "persistManagers", true);
            initSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject saveSerialized = new SerializedObject(saveSystemManager);
            SetIfExists(saveSerialized, "dialogueManager", dialogueManager);
            SetIfExists(saveSerialized, "relationshipManager", relationshipManager);
            saveSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject flowSerialized = new SerializedObject(gameManager.GetComponent<GameFlowStateMachine>());
            SetIfExists(flowSerialized, "dialogueManager", dialogueManager);
            SetIfExists(flowSerialized, "saveSystemManager", saveSystemManager);
            flowSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject connectorSerialized = new SerializedObject(gameManager.GetComponent<DialogueGameplayConnector>());
            SetIfExists(connectorSerialized, "dialogueManager", dialogueManager);
            SetIfExists(connectorSerialized, "relationshipManager", relationshipManager);
            SetIfExists(connectorSerialized, "gameFlowStateMachine", gameManager.GetComponent<GameFlowStateMachine>());
            connectorSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject dialogueSerialized = new SerializedObject(dialogueManager);
            SetIfExists(dialogueSerialized, "typewriterEffect", typewriterEffect);
            dialogueSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildSceneUi(
            Transform canvasRoot,
            DialogueManager dialogueManager,
            TypewriterEffect typewriterEffect,
            SaveSystemManager saveSystemManager,
            AudioManager audioManager)
        {
            GameObject backgroundLayer = GetOrCreateUiChild(canvasRoot, "BackgroundLayer");
            StretchToParent(backgroundLayer.GetComponent<RectTransform>());

            GameObject backgroundFrom = GetOrCreateUiChild(backgroundLayer.transform, "BackgroundFrom", typeof(Image));
            RectTransform bgFromRect = backgroundFrom.GetComponent<RectTransform>();
            StretchToParent(bgFromRect);
            Image bgFromImage = backgroundFrom.GetComponent<Image>();
            bgFromImage.color = new Color(0.16f, 0.36f, 0.68f, 1f);

            GameObject backgroundTo = GetOrCreateUiChild(backgroundLayer.transform, "BackgroundTo", typeof(Image));
            RectTransform bgToRect = backgroundTo.GetComponent<RectTransform>();
            StretchToParent(bgToRect);
            Image bgToImage = backgroundTo.GetComponent<Image>();
            bgToImage.color = new Color(0.16f, 0.36f, 0.68f, 0f);

            GameObject characterLayer = GetOrCreateUiChild(canvasRoot, "CharacterLayer");
            StretchToParent(characterLayer.GetComponent<RectTransform>());

            GameObject leftPortrait = GetOrCreateUiChild(characterLayer.transform, "LeftPortrait", typeof(Image), typeof(CanvasGroup));
            ConfigurePortraitRect(leftPortrait.GetComponent<RectTransform>(), true);
            leftPortrait.GetComponent<Image>().color = new Color(0.95f, 0.53f, 0.70f, 1f);

            GameObject rightPortrait = GetOrCreateUiChild(characterLayer.transform, "RightPortrait", typeof(Image), typeof(CanvasGroup));
            ConfigurePortraitRect(rightPortrait.GetComponent<RectTransform>(), false);
            rightPortrait.GetComponent<Image>().color = new Color(0.38f, 0.58f, 0.92f, 1f);

            GameObject dialoguePanel = GetOrCreateUiChild(canvasRoot, "DialoguePanel", typeof(Image));
            RectTransform dialoguePanelRect = dialoguePanel.GetComponent<RectTransform>();
            dialoguePanelRect.anchorMin = new Vector2(0.03f, 0.02f);
            dialoguePanelRect.anchorMax = new Vector2(0.97f, 0.30f);
            dialoguePanelRect.offsetMin = Vector2.zero;
            dialoguePanelRect.offsetMax = Vector2.zero;
            dialoguePanel.GetComponent<Image>().color = new Color(0.10f, 0.10f, 0.14f, 0.92f);

            GameObject characterNameTextGo = GetOrCreateText(dialoguePanel.transform, "CharacterNameText", "Character");
            RectTransform nameRect = characterNameTextGo.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.02f, 0.68f);
            nameRect.anchorMax = new Vector2(0.36f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            TextMeshProUGUI nameText = characterNameTextGo.GetComponent<TextMeshProUGUI>();
            nameText.fontSize = 42f;
            nameText.alignment = TextAlignmentOptions.Left;

            GameObject dialogueTextGo = GetOrCreateText(dialoguePanel.transform, "DialogueText", "Dialogue text appears here.");
            RectTransform dialogueRect = dialogueTextGo.GetComponent<RectTransform>();
            dialogueRect.anchorMin = new Vector2(0.02f, 0.10f);
            dialogueRect.anchorMax = new Vector2(0.98f, 0.66f);
            dialogueRect.offsetMin = Vector2.zero;
            dialogueRect.offsetMax = Vector2.zero;
            TextMeshProUGUI dialogueText = dialogueTextGo.GetComponent<TextMeshProUGUI>();
            dialogueText.fontSize = 36f;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;

            GameObject continueIndicator = GetOrCreateUiChild(dialoguePanel.transform, "ContinueIndicator");
            RectTransform indicatorRect = continueIndicator.GetComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(0.92f, 0.03f);
            indicatorRect.anchorMax = new Vector2(0.99f, 0.15f);
            indicatorRect.offsetMin = Vector2.zero;
            indicatorRect.offsetMax = Vector2.zero;
            GameObject continueTextGo = GetOrCreateText(continueIndicator.transform, "ContinueIndicatorText", ">>");
            StretchToParent(continueTextGo.GetComponent<RectTransform>());
            TextMeshProUGUI continueText = continueTextGo.GetComponent<TextMeshProUGUI>();
            continueText.fontSize = 38f;
            continueText.alignment = TextAlignmentOptions.Center;

            GameObject choiceContainer = GetOrCreateUiChild(canvasRoot, "ChoiceContainer");
            RectTransform choiceRect = choiceContainer.GetComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(0.20f, 0.34f);
            choiceRect.anchorMax = new Vector2(0.80f, 0.62f);
            choiceRect.offsetMin = Vector2.zero;
            choiceRect.offsetMax = Vector2.zero;
            VerticalLayoutGroup vlg = EnsureComponent<VerticalLayoutGroup>(choiceContainer);
            vlg.spacing = 12f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            ContentSizeFitter fitter = EnsureComponent<ContentSizeFitter>(choiceContainer);
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            GameObject transitionPanel = GetOrCreateUiChild(canvasRoot, "TransitionPanel", typeof(Image), typeof(CanvasGroup));
            StretchToParent(transitionPanel.GetComponent<RectTransform>());
            Image transitionImage = transitionPanel.GetComponent<Image>();
            transitionImage.color = new Color(0f, 0f, 0f, 0f);
            CanvasGroup transitionCanvasGroup = transitionPanel.GetComponent<CanvasGroup>();
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.interactable = false;
            transitionCanvasGroup.blocksRaycasts = false;

            GameObject saveLoadUi = GetOrCreateUiChild(canvasRoot, "SaveLoadUI");
            RectTransform saveLoadRect = saveLoadUi.GetComponent<RectTransform>();
            saveLoadRect.anchorMin = new Vector2(0.78f, 0.84f);
            saveLoadRect.anchorMax = new Vector2(0.98f, 0.98f);
            saveLoadRect.offsetMin = Vector2.zero;
            saveLoadRect.offsetMax = Vector2.zero;

            GameObject saveButton = GetOrCreateUiChild(saveLoadUi.transform, "SaveButton", typeof(Image), typeof(Button));
            RectTransform saveButtonRect = saveButton.GetComponent<RectTransform>();
            saveButtonRect.anchorMin = new Vector2(0.05f, 0.52f);
            saveButtonRect.anchorMax = new Vector2(0.95f, 0.95f);
            saveButtonRect.offsetMin = Vector2.zero;
            saveButtonRect.offsetMax = Vector2.zero;
            saveButton.GetComponent<Image>().color = new Color(0.19f, 0.25f, 0.36f, 1f);
            GameObject saveButtonText = GetOrCreateText(saveButton.transform, "Text", "Save");
            StretchToParent(saveButtonText.GetComponent<RectTransform>());
            saveButtonText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            GameObject loadButton = GetOrCreateUiChild(saveLoadUi.transform, "LoadButton", typeof(Image), typeof(Button));
            RectTransform loadButtonRect = loadButton.GetComponent<RectTransform>();
            loadButtonRect.anchorMin = new Vector2(0.05f, 0.05f);
            loadButtonRect.anchorMax = new Vector2(0.95f, 0.48f);
            loadButtonRect.offsetMin = Vector2.zero;
            loadButtonRect.offsetMax = Vector2.zero;
            loadButton.GetComponent<Image>().color = new Color(0.19f, 0.25f, 0.36f, 1f);
            GameObject loadButtonText = GetOrCreateText(loadButton.transform, "Text", "Load");
            StretchToParent(loadButtonText.GetComponent<RectTransform>());
            loadButtonText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            GameObject autoSaveIndicator = GetOrCreateUiChild(saveLoadUi.transform, "AutoSaveIndicator");
            RectTransform autoSaveRect = autoSaveIndicator.GetComponent<RectTransform>();
            autoSaveRect.anchorMin = new Vector2(0.05f, -0.35f);
            autoSaveRect.anchorMax = new Vector2(0.95f, 0.0f);
            autoSaveRect.offsetMin = Vector2.zero;
            autoSaveRect.offsetMax = Vector2.zero;
            GameObject autoSaveText = GetOrCreateText(autoSaveIndicator.transform, "Text", "Saved");
            StretchToParent(autoSaveText.GetComponent<RectTransform>());
            autoSaveText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            autoSaveIndicator.SetActive(false);

            EnsureComponent<BackgroundTransitionPresenter>(backgroundLayer);
            EnsureComponent<CharacterPortraitPresenter>(characterLayer);
            EnsureComponent<PortraitFocusPresenter>(characterLayer);
            EnsureComponent<ContinueIndicatorPresenter>(dialoguePanel);
            EnsureComponent<SaveLoadUiController>(saveLoadUi);
            EnsureComponent<TransitionPanelController>(transitionPanel);
            DialogueInputController inputController = EnsureComponent<DialogueInputController>(canvasRoot.gameObject);

            ConfigureSceneReferences(
                dialogueManager,
                typewriterEffect,
                dialogueText,
                nameText,
                choiceContainer.transform,
                continueIndicator,
                continueText,
                saveLoadUi.GetComponent<SaveLoadUiController>(),
                saveSystemManager,
                saveButton.GetComponent<Button>(),
                loadButton.GetComponent<Button>(),
                autoSaveIndicator,
                autoSaveText.GetComponent<TextMeshProUGUI>(),
                backgroundLayer.GetComponent<BackgroundTransitionPresenter>(),
                bgFromImage,
                bgToImage,
                characterLayer.GetComponent<CharacterPortraitPresenter>(),
                characterLayer.GetComponent<PortraitFocusPresenter>(),
                leftPortrait.GetComponent<Image>(),
                rightPortrait.GetComponent<Image>(),
                transitionPanel.GetComponent<TransitionPanelController>(),
                transitionCanvasGroup,
                gameFlowStateMachine: Object.FindFirstObjectByType<GameFlowStateMachine>(),
                inputController,
                audioManager);
        }

        private static void ConfigureSceneReferences(
            DialogueManager dialogueManager,
            TypewriterEffect typewriterEffect,
            TextMeshProUGUI dialogueText,
            TextMeshProUGUI nameText,
            Transform choiceContainer,
            GameObject continueIndicator,
            TextMeshProUGUI continueText,
            SaveLoadUiController saveLoadUiController,
            SaveSystemManager saveSystemManager,
            Button saveButton,
            Button loadButton,
            GameObject autoSaveIndicator,
            TextMeshProUGUI autoSaveText,
            BackgroundTransitionPresenter backgroundPresenter,
            Image backgroundFromImage,
            Image backgroundToImage,
            CharacterPortraitPresenter characterPortraitPresenter,
            PortraitFocusPresenter portraitFocusPresenter,
            Image leftPortrait,
            Image rightPortrait,
            TransitionPanelController transitionPanelController,
            CanvasGroup transitionCanvasGroup,
            GameFlowStateMachine gameFlowStateMachine,
            DialogueInputController inputController,
            AudioManager audioManager)
        {
            SerializedObject dialogueSerialized = new SerializedObject(dialogueManager);
            SetIfExists(dialogueSerialized, "characterNameText", nameText);
            SetIfExists(dialogueSerialized, "dialogueText", dialogueText);
            SetIfExists(dialogueSerialized, "choicesContainer", choiceContainer);
            SetIfExists(dialogueSerialized, "typewriterEffect", typewriterEffect);
            dialogueSerialized.ApplyModifiedPropertiesWithoutUndo();

            ContinueIndicatorPresenter indicatorPresenter = continueIndicator.transform.parent.GetComponent<ContinueIndicatorPresenter>();
            SerializedObject indicatorSerialized = new SerializedObject(indicatorPresenter);
            SetIfExists(indicatorSerialized, "dialogueManager", dialogueManager);
            SetIfExists(indicatorSerialized, "typewriterEffect", typewriterEffect);
            SetIfExists(indicatorSerialized, "indicatorRoot", continueIndicator);
            SetIfExists(indicatorSerialized, "indicatorText", continueText);
            SetIfExists(indicatorSerialized, "choicesContainer", choiceContainer);
            indicatorSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject saveUiSerialized = new SerializedObject(saveLoadUiController);
            SetIfExists(saveUiSerialized, "saveSystemManager", saveSystemManager);
            SetIfExists(saveUiSerialized, "saveButton", saveButton);
            SetIfExists(saveUiSerialized, "loadButton", loadButton);
            SetIfExists(saveUiSerialized, "autoSaveIndicatorRoot", autoSaveIndicator);
            SetIfExists(saveUiSerialized, "autoSaveIndicatorText", autoSaveText);
            saveUiSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject backgroundSerialized = new SerializedObject(backgroundPresenter);
            SetIfExists(backgroundSerialized, "dialogueManager", dialogueManager);
            SetIfExists(backgroundSerialized, "fromImage", backgroundFromImage);
            SetIfExists(backgroundSerialized, "toImage", backgroundToImage);
            backgroundSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject portraitSerialized = new SerializedObject(characterPortraitPresenter);
            SetIfExists(portraitSerialized, "dialogueManager", dialogueManager);
            portraitSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject focusSerialized = new SerializedObject(portraitFocusPresenter);
            SetIfExists(focusSerialized, "dialogueManager", dialogueManager);
            focusSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject transitionSerialized = new SerializedObject(transitionPanelController);
            SetIfExists(transitionSerialized, "gameFlowStateMachine", gameFlowStateMachine);
            SetIfExists(transitionSerialized, "transitionCanvasGroup", transitionCanvasGroup);
            transitionSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject inputSerialized = new SerializedObject(inputController);
            SetIfExists(inputSerialized, "dialogueManager", dialogueManager);
            SetIfExists(inputSerialized, "audioManager", audioManager);
            inputSerialized.ApplyModifiedPropertiesWithoutUndo();

            EnsureComponent<CanvasGroup>(leftPortrait.gameObject);
            EnsureComponent<CanvasGroup>(rightPortrait.gameObject);
        }

        private static GameObject GetOrCreateRoot(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            }

            return go;
        }

        private static GameObject GetOrCreateChild(GameObject parent, string name)
        {
            Transform existing = parent.transform.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(child, $"Create {name}");
            child.transform.SetParent(parent.transform, false);
            return child;
        }

        private static GameObject GetOrCreateUiChild(Transform parent, string name, params System.Type[] additionalComponents)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent, false);

            for (int i = 0; i < additionalComponents.Length; i++)
            {
                EnsureComponent(go, additionalComponents[i]);
            }

            return go;
        }

        private static GameObject GetOrCreateText(Transform parent, string name, string value)
        {
            GameObject textGo = GetOrCreateUiChild(parent, name, typeof(TextMeshProUGUI));
            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = value;
            tmp.color = Color.white;
            return textGo;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void ConfigurePortraitRect(RectTransform rect, bool left)
        {
            if (left)
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.anchoredPosition = new Vector2(40f, 0f);
            }
            else
            {
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.anchoredPosition = new Vector2(-40f, 0f);
            }

            rect.sizeDelta = new Vector2(520f, 0f);
            rect.offsetMin = new Vector2(rect.offsetMin.x, 220f);
            rect.offsetMax = new Vector2(rect.offsetMax.x, -40f);
        }

        private static AudioSource GetOrCreateNamedAudioSource(GameObject parent, string name, bool loop)
        {
            Transform existing = parent.transform.Find(name);
            AudioSource source;
            if (existing != null)
            {
                source = EnsureComponent<AudioSource>(existing.gameObject);
            }
            else
            {
                GameObject sourceGo = new GameObject(name, typeof(AudioSource));
                Undo.RegisterCreatedObjectUndo(sourceGo, $"Create {name}");
                sourceGo.transform.SetParent(parent.transform, false);
                source = sourceGo.GetComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = Undo.AddComponent<T>(go);
            }

            return component;
        }

        private static Component EnsureComponent(GameObject go, System.Type type)
        {
            Component component = go.GetComponent(type);
            if (component == null)
            {
                component = Undo.AddComponent(go, type);
            }

            return component;
        }

        private static void SetIfExists(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void SetIfExists(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }
    }
}
