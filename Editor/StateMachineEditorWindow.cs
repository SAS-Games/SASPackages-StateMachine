using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    [ExecuteInEditMode]
    internal class StateMachineEditorWindow : GridEditorWindow
    {
        private const float DebugToolbarWidth = 430f;
        private const float ToolbarHeight = 20f;

        protected RuntimeStateMachineController _runtimeStateMachineController;
        private StateMachineParameterEditor _parameterEditor = new StateMachineParameterEditor();
        private StateTransitionEditor _transition;

        private List<BaseNode> _nodes = new List<BaseNode>();
        private Actor Actor => Selection.activeGameObject?.GetComponent<Actor>();

        private List<StateMachineModel> _selectedChildStateMachines = new List<StateMachineModel>();
        private StateMachineModel SelectedStateMachineModel
        {
            get
            {
                if (_selectedChildStateMachines.Count > 0)
                    return _selectedChildStateMachines[_selectedChildStateMachines.Count - 1];
                return null;
            }
        }

        public static StateMachineOverrideController SelectedStateMachineOverrideController;
        public static bool IsReadOnlyMode => SelectedStateMachineOverrideController != null;
        private bool IsRuntimeControllerReadOnly => _runtimeStateMachineController != null && !_runtimeStateMachineController.IsAssetBacked();
        private bool IsGraphReadOnly => IsReadOnlyMode || IsRuntimeControllerReadOnly;

        public static void ShowBehaviourGraphEditor(RuntimeStateMachineController target)
        {
            var detailsWindow = GetWindow<StateMachineEditorWindow>(typeof(SceneView));
            GUIContent content = new GUIContent("StateMachine");
            detailsWindow.titleContent = content;
            detailsWindow.ShowTab();
            detailsWindow.Repaint();
            Selection.activeObject = target;
            EditorPrefs.SetString("StateMachine", AssetDatabase.GetAssetPath(target));
        }

        public void Awake()
        {
            OnSelectionChange();
        }

        private void OnFocus()
        {
            OnSelectionChange();
        }

        void OnSelectionChange()
        {
            if (Selection.activeObject == null)
                return;
            
            var stateMachineModel = Selection.activeObject as RuntimeStateMachineController;
            if (Selection.activeObject is StateMachineOverrideController)
            {
                SelectedStateMachineOverrideController = (stateMachineModel as StateMachineOverrideController);
                stateMachineModel = (stateMachineModel as StateMachineOverrideController).runtimeStateMachineController;
            }
            else if (Selection.activeObject is RuntimeStateMachineController)
                SelectedStateMachineOverrideController = null;

            if (stateMachineModel == null)
            {
                if (Actor != null)
                {
                    var actorSO = new SerializedObject(Actor);
                    stateMachineModel = actorSO.FindProperty("m_Controller").objectReferenceValue as RuntimeStateMachineController;
                    if (stateMachineModel is StateMachineOverrideController)
                    {
                        SelectedStateMachineOverrideController = (stateMachineModel as StateMachineOverrideController);
                        stateMachineModel = (stateMachineModel as StateMachineOverrideController).runtimeStateMachineController;
                    }
                    else
                        SelectedStateMachineOverrideController = null;
                }
            }
            else
                EditorPrefs.SetString("StateMachine", AssetDatabase.GetAssetPath(stateMachineModel));

            if (stateMachineModel != null)
            {
                _runtimeStateMachineController = stateMachineModel.ResolveAssetBackedController();
                if (_runtimeStateMachineController == null)
                {
                    if (EditorPrefs.HasKey("StateMachine"))
                        _runtimeStateMachineController = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("StateMachine"), typeof(RuntimeStateMachineController)) as RuntimeStateMachineController;
                    if (_runtimeStateMachineController == null)
                        return;
                }

                Initialize();
            }
        }

        protected void OnEnable()
        {
            StateMachineDebugSession.Instance.Changed += Repaint;
            EditorApplication.pauseStateChanged += OnEditorPauseStateChanged;
            EditorApplication.playModeStateChanged += OnEditorPlayModeStateChanged;
            OnSelectionChange();
        }

        private void OnDisable()
        {
            StateMachineDebugSession.Instance.Changed -= Repaint;
            EditorApplication.pauseStateChanged -= OnEditorPauseStateChanged;
            EditorApplication.playModeStateChanged -= OnEditorPlayModeStateChanged;
        }

        private void OnEditorPauseStateChanged(PauseState pauseState)
        {
            Repaint();
        }

        private void OnEditorPlayModeStateChanged(PlayModeStateChange playModeState)
        {
            Repaint();
        }

        private void Initialize()
        {
            if (_runtimeStateMachineController.IsAssetBacked())
            {
                foreach (var stateMachineModel in _runtimeStateMachineController.GetAllStateMachines())
                    _runtimeStateMachineController.EnsureEntryExitNodes(stateMachineModel);
                _runtimeStateMachineController.SyncRootEntryTransitionToDefault();
            }

            _selectedChildStateMachines.Clear();
            _selectedChildStateMachines.Add(_runtimeStateMachineController.BaseStateMachineModel());

            _parameterEditor = new StateMachineParameterEditor(_runtimeStateMachineController);
            _transition = new StateTransitionEditor(_runtimeStateMachineController);

            CreateSelectedStateMachineNodes();

        }

        private void CreateSelectedStateMachineNodes()
        {
            _transition.Clear();
            _nodes.Clear();

            if (SelectedStateMachineModel == null)
                return;

            var anyStateNode = new AnyStateNode(SelectedStateMachineModel, _runtimeStateMachineController.AnyStateModel(), SelectedStateMachineModel.GetAnyStatePosition(), StartTranstionFromAnyState);
            _nodes.Add(anyStateNode);

            CreateEntryExitNodes();

            var stateMachineModels = SelectedStateMachineModel.GetChildStateMachines();
            for (int i = 0; i < stateMachineModels.Count; ++i)
                CreateChildMachinelNode(stateMachineModels[i]);

            var stateModels = SelectedStateMachineModel.GetStates();
            for (int i = 0; i < stateModels.Count; ++i)
                CreateStateModelNode(stateModels[i]);

            var parentStateModel = SelectedStateMachineModel.GetParent();
            if (parentStateModel != null)
                CreateParentMachinelNode(parentStateModel);

            CreateTransitions();
            Repaint();
        }

        private void CreateEntryExitNodes()
        {
            var entryNode = _runtimeStateMachineController.GetAssetBackedEntryNode(SelectedStateMachineModel);
            if (entryNode != null)
                _nodes.Add(new EntryStateNode(entryNode, (Vector2Int)entryNode.GetPosition(), StartTranstion));

            var exitNode = _runtimeStateMachineController.GetAssetBackedExitNode(SelectedStateMachineModel);
            if (exitNode != null)
                _nodes.Add(new ExitStateNode(exitNode, (Vector2Int)exitNode.GetPosition(), MakeTranstion));
        }

        private void CreateTransitions()
        {
            foreach (BaseNode sourceNode in _nodes)
            {
                var sourceNodeModel = GetTransitionNodeModel(sourceNode);
                if (sourceNodeModel != null)
                    CreateTransitions(sourceNode, sourceNodeModel);
            }
        }

        private void CreateTransitions(BaseNode sourceNode, TransitionNodeModel sourceStateModel)
        {
            if (sourceStateModel == null)
                return;

            var stateTransitions = sourceStateModel?.GetTransitionsProp();
            if (stateTransitions == null)
                return;

            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = stateTransitions.GetArrayElementAtIndex(i).objectReferenceValue as StateTransitionModel;
                if (element == null)
                    continue;

                var targetStateModel = element.TargetNodeModel;
                if (targetStateModel == null)
                    continue;

                var targetNode = _nodes.Find(ele => ele.TargetObject == targetStateModel);
                if (targetNode == null)
                {
                    //check if target state belongs to any childStateMachine
                    if (sourceNode is AnyStateNode)
                        continue;
                    foreach (BaseNode node in _nodes)
                    {
                        if (node is StateMachineNode stateMachineNode)
                        {
                            if (targetStateModel is StateModel stateModel && stateMachineNode.Value.Contains(stateModel))
                            {
                                targetNode = node;
                                break;
                            }
                        }
                    }
                }
                _transition.Add(sourceNode, targetNode, sourceStateModel, targetStateModel);
            }
        }

        protected override void OnGUI()
        {
            var rect = position;
            rect.x -= position.x;
            rect.y -= position.y;
            rect.height -= 2;

            EditorZoomArea.Begin(Zoom, rect);
            base.OnGUI();
            DrawStateMachineWindow();
            EditorZoomArea.End();

            DrawToolBar();
            DrawParameterWindow();
            

            if (IsGraphReadOnly)
                DrawReadOnlyOverlay();

            EditorUtilities.VerticalLine(new Rect(Mathf.Max(200, position.width / 5) - 2, 1, position.width, position.height), 2, Color.black);
        }
        
        private void DrawReadOnlyOverlay()
        {
            var rect = new Rect(0, 0, position.width, 24);
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.25f));
            GUI.Label(rect, IsRuntimeControllerReadOnly ? "Runtime Controller - Read Only" : "Override Controller - Read Only", EditorStyles.boldLabel);
        }


        void OnInspectorUpdate()
        {
            this.Repaint();
        }

        private void DrawStateMachineWindow()
        {
            if (_runtimeStateMachineController == null)
                return;

            SetCurrentActiveNode();

            var actor = Actor;
            var graphName = SelectedStateMachineModel != null ? SelectedStateMachineModel.name : string.Empty;
            _transition?.DrawConnectionLine(Event.current);
            _transition?.DrawConnections();
            _transition?.DrawDebugOverlays(actor, graphName);
            DrawNodes(actor, graphName);
            ProcessNodeEvents(Event.current, actor, graphName);
            _transition?.ProcessConnectionEvents(Event.current, IsGraphReadOnly);
            ProcessMouseEvent(Event.current);
        }

        private void DrawToolBar()
        {
            EditorUtilities.HorizontalLine(new Rect(0, -2, position.width, 21), 22, new Color(0.2196079f, 0.2196079f, 0.2196079f));
            var debugToolbarWidth = Mathf.Min(DebugToolbarWidth, Mathf.Max(0f, position.width - Mathf.Max(200, position.width / 5) - 8f));
            DrawStateMachineToolBar(new Rect(0, -1, position.width - debugToolbarWidth - 4f, ToolbarHeight));
            DrawDebugToolBar(new Rect(position.width - debugToolbarWidth - 2f, -1, debugToolbarWidth, ToolbarHeight));
            EditorUtilities.HorizontalLine(new Rect(0, 20, position.width, 21), 1, Color.black);
        }

        private void DrawParameterWindow()
        {
            BeginWindows();

            var parameterController = GetParameterController();
            if (_parameterEditor == null || _parameterEditor.TargetController != parameterController)
                _parameterEditor = new StateMachineParameterEditor(parameterController);

            var windowRect = GUI.Window(1, new Rect(0, -2, Mathf.Max(200, position.width / 5), position.height - 2), id=>_parameterEditor.DrawParametersWindow(id, IsParameterWindowReadOnly(parameterController)), "", new GUIStyle());
            _parameterEditor.DrawRect(windowRect);
            EndWindows();
        }

        private RuntimeStateMachineController GetParameterController()
        {
            var actor = Actor;
            if (Application.isPlaying && actor != null && actor.runtimeStateMachineController != null)
                return actor.runtimeStateMachineController;

            return _runtimeStateMachineController;
        }

        private bool IsParameterWindowReadOnly(RuntimeStateMachineController parameterController)
        {
            if (Application.isPlaying && parameterController != null && !parameterController.IsAssetBacked())
                return true;

            return IsGraphReadOnly;
        }

        int selectedIndex = 0;
        private void DrawStateMachineToolBar(Rect rect)
        {
            if (SelectedStateMachineModel == null)
                return;

            rect.x = Mathf.Max(200, position.width / 5) - 10;
            rect.width = Mathf.Max(0f, rect.width - rect.x);
            if (rect.width <= 20f)
                return;

            var childStateMachines = _selectedChildStateMachines.Select(ele => ele.name).ToArray();

            selectedIndex = GUI.Toolbar(rect, _selectedChildStateMachines.Count - 1, childStateMachines, Settings.ChildStateMachinestoolBarStyle);

            if (selectedIndex != _selectedChildStateMachines.Count - 1)
            {
                _selectedChildStateMachines.RemoveRange(selectedIndex + 1, (_selectedChildStateMachines.Count - selectedIndex) - 1);
                CreateSelectedStateMachineNodes();
            }
        }

        private void DrawDebugToolBar(Rect rect)
        {
            if (rect.width <= 260f)
                return;

            var session = StateMachineDebugSession.Instance;
            GUI.BeginGroup(rect);
            try
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var enabled = GUILayout.Toggle(session.IsEnabled, "Debug", EditorStyles.toolbarButton, GUILayout.Width(60));
                    if (enabled != session.IsEnabled)
                    {
                        if (enabled)
                            session.Enable();
                        else
                            session.Disable();
                    }

                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(45)))
                        session.Clear();

                    if (GUILayout.Button("Clear BP", EditorStyles.toolbarButton, GUILayout.Width(65)))
                        session.ClearBreakpoints();

                    using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
                    {
                        var pauseLabel = EditorApplication.isPaused ? "Resume" : "Pause";
                        if (GUILayout.Button(pauseLabel, EditorStyles.toolbarButton, GUILayout.Width(65)))
                        {
                            if (EditorApplication.isPaused)
                                session.Resume();
                            else
                                session.Pause();
                        }

                        using (new EditorGUI.DisabledScope(!EditorApplication.isPaused))
                        {
                            if (GUILayout.Button("Next Frame", EditorStyles.toolbarButton, GUILayout.Width(80)))
                                session.StepFrame();
                        }
                    }

#if !SAS_STATE_MACHINE_DEBUG
                    GUILayout.Label("No Symbol", EditorStyles.miniLabel, GUILayout.Width(65));
#else
                    GUILayout.Label($"BP {session.BreakpointCount}", EditorStyles.miniLabel, GUILayout.Width(40));
#endif
                }
            }
            finally
            {
                GUI.EndGroup();
            }
        }

        protected override void ProcessMouseEvent(Event e)
        {
            base.ProcessMouseEvent(e);
            switch (e.type)
            {
                case EventType.MouseUp:
                    if (e.button == 1)
                        ProcessContextMenu(e.mousePosition);

                    else if (e.button == 0)
                        _transition.ClearConnectionSelection();
                    break;
            }
        }

        private void ProcessNodeEvents(Event e, Actor actor, string graphName)
        {
            if (_nodes != null)
            {
                for (int i = _nodes.Count - 1; i >= 0 && i < _nodes.Count; i--)
                {
                    _nodes[i].SetDebugContext(actor, graphName);
                    if (_nodes[i].ProcessEvents(e, IsGraphReadOnly))
                    {
                        Selection.activeObject = _nodes[i].TargetObject;
                        GUI.changed = true;
                        e.Use();
                    }
                }
            }
        }

        private void DrawNodes(Actor actor, string graphName)
        {
            foreach (BaseNode node in _nodes)
            {
                node.SetDebugContext(actor, graphName);
                node.Draw();
                StateMachineDebugOverlay.DrawNodeOverlay(node, actor, graphName);
            }
        }

        protected override void OnDrag(Vector2 delta)
        {
            base.OnDrag(delta);
            if (_nodes != null)
            {
                if (!IsGraphReadOnly)
                {
                    for (int i = 0; i < _nodes.Count; i++)
                        _nodes[i].Drag(delta.ToVector2Int());
                }
            }
            GUI.changed = true;
        }

        private void ProcessContextMenu(Vector2 mousePosition)
        {
            if (IsGraphReadOnly)
                return;
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Create State"), false, () => AddState(mousePosition));
            genericMenu.AddItem(new GUIContent("Create Sub-State Machine"), false, () => AddChildStateMachine(mousePosition));
            genericMenu.AddItem(new GUIContent("Duplicate current StateMachine"), false, () => DuplicateCurrentStateMachine(mousePosition));
            genericMenu.ShowAsContext();
        }

        private void AddState(Vector2 mousePosition)
        {
            if (IsGraphReadOnly)
                return;

            _runtimeStateMachineController.AddState(SelectedStateMachineModel, "New State", mousePosition.ToVector3Int());
            CreateSelectedStateMachineNodes();
        }

        private void AddChildStateMachine(Vector2 mousePosition)
        {
            if (IsGraphReadOnly)
                return;

            var stateMachineModel = _runtimeStateMachineController.AddChildStateMachine(SelectedStateMachineModel, "New StateMachine", new Vector3Int((int)mousePosition.x,(int)mousePosition.y, 0));
            CreateChildMachinelNode(stateMachineModel);
        }

        private void DuplicateCurrentStateMachine(Vector2 mousePosition)
        {
            if (IsGraphReadOnly)
                return;

            CreateChildMachinelNode(SelectedStateMachineModel.CloneMachineRecursivily(_runtimeStateMachineController, SelectedStateMachineModel, new Vector3Int((int)mousePosition.x,(int)mousePosition.y, 0)));
        }

        private void DuplicateStateMachine(StateMachineNode stateMachineNode)
        {
            if (IsGraphReadOnly)
                return;

            CreateChildMachinelNode(stateMachineNode.Value.CloneMachineRecursivily(_runtimeStateMachineController, SelectedStateMachineModel));
        }

        private void CreateStateModelNode(StateModel stateModel)
        {
            if (stateModel == null)
                return;

            StateNode node;
            bool isDefaultState = (stateModel == _runtimeStateMachineController.GetDefaultState() || _runtimeStateMachineController.GetAllStateModels().Count == 1);
            Action<StateNode> action = RemoveStateModelNode;
            if (isDefaultState)
                action = RemoveDefaultStateModelNode;
            node = new StateNode(stateModel, (Vector2Int)stateModel.GetPosition(), isDefaultState, StartTranstion, MakeTranstion, action, SetAsDefaultNode, DuplicateNode);
            if (isDefaultState && _runtimeStateMachineController.IsAssetBacked())
                SetAsDefaultNode(node, false);

            _nodes.Add(node);
        }

        private void CreateChildMachinelNode(StateMachineModel stateMachineModel)
        {
            if (stateMachineModel == null)
                return;

            var node = new StateMachineNode(stateMachineModel, (Vector2Int)stateMachineModel.GetPosition(), _runtimeStateMachineController.IsDefaultStateMachine(stateMachineModel), StartTranstion, MakeTranstion, RemoveStateMachineNode, SelectStateMachineNode, DuplicateStateMachine);
            _nodes.Add(node);
            Repaint();
        }

        private void CreateParentMachinelNode(StateMachineModel stateMachineModel)
        {
            var node = new ParentStateMachineNode(stateMachineModel, (Vector2Int)stateMachineModel.GetPositionAsUpNode(), _runtimeStateMachineController.IsDefaultStateMachine(stateMachineModel), StartTranstion, MakeTranstion, GoToMachineNode);
            _nodes.Add(node);
        }

        private void StartTranstionFromAnyState(BaseNode sourceNode)
        {
            if (IsGraphReadOnly)
                return;

            var stateNode = sourceNode as AnyStateNode;
            _transition.Start(stateNode, stateNode.Value);
        }

        private void StartTranstion(BaseNode sourceNode)
        {
            if (IsGraphReadOnly)
                return;

            var sourceNodeModel = GetTransitionNodeModel(sourceNode);
            if (sourceNodeModel != null)
                _transition.Start(sourceNode, sourceNodeModel);
        }

        private void MakeTranstion(BaseNode targetNode)
        {
            if (IsGraphReadOnly)
                return;

            if (targetNode is StateMachineNode stateMachineNode && !(targetNode is ParentStateMachineNode))
            {
                MakeTransitionToStateMachine(stateMachineNode);
                Repaint();
                return;
            }

            var targetNodeModel = GetTransitionNodeModel(targetNode);
            if (targetNodeModel != null)
                MakeTranstion(targetNode, targetNodeModel);
            Repaint();
        }

        private void MakeTransitionToStateMachine(StateMachineNode targetNode)
        {
            if (IsGraphReadOnly)
                return;

            if (_transition.SourceNodeModel == null)
                return;

            var genericMenu = new GenericMenu();
            AddStateTargetMenuItems(genericMenu, targetNode, targetNode.Value, "States");
            AddStateMachineTargetMenuItems(genericMenu, targetNode, targetNode.Value, "StateMachines");
            genericMenu.ShowAsContext();
        }

        private void AddStateTargetMenuItems(GenericMenu genericMenu, BaseNode targetNode, StateMachineModel stateMachineModel, string path)
        {
            var states = stateMachineModel.GetStates();
            for (int i = 0; i < states.Count; ++i)
            {
                var state = states[i];
                genericMenu.AddItem(new GUIContent($"{path}/{state.name}"), false, () => MakeTranstion(targetNode, state));
            }

            var childStateMachines = stateMachineModel.GetChildStateMachines();
            for (int i = 0; i < childStateMachines.Count; ++i)
            {
                var childStateMachine = childStateMachines[i];
                AddStateTargetMenuItems(genericMenu, targetNode, childStateMachine, $"{path}/{childStateMachine.name}");
            }
        }

        private void AddStateMachineTargetMenuItems(GenericMenu genericMenu, BaseNode targetNode, StateMachineModel stateMachineModel, string path)
        {
            genericMenu.AddItem(new GUIContent($"{path}/{stateMachineModel.name}"), false, () => MakeTranstion(targetNode, stateMachineModel));

            var childStateMachines = stateMachineModel.GetChildStateMachines();
            for (int i = 0; i < childStateMachines.Count; ++i)
            {
                var childStateMachine = childStateMachines[i];
                AddStateMachineTargetMenuItems(genericMenu, targetNode, childStateMachine, $"{path}/{stateMachineModel.name}");
            }
        }

        private void MakeTranstion(BaseNode targetNode, TransitionNodeModel targetStateModel)
        {
            if (IsGraphReadOnly)
                return;

            _transition.Make(targetNode, targetStateModel);
        }

        private TransitionNodeModel GetTransitionNodeModel(BaseNode node)
        {
            if (node is ParentStateMachineNode)
                return null;
            if (node is StateNode stateNode)
                return stateNode.Value;
            if (node is AnyStateNode anyStateNode)
                return anyStateNode.Value;
            if (node is StateMachineNode stateMachineNode)
                return stateMachineNode.Value;
            if (node is EntryStateNode entryStateNode)
                return entryStateNode.Value;
            if (node is ExitStateNode exitStateNode)
                return exitStateNode.Value;

            return null;
        }

        private void RemoveStateMachineNode(StateMachineNode stateMachineNode)
        {
            if (IsGraphReadOnly)
                return;

            _runtimeStateMachineController.RemoveStateMachine(stateMachineNode.Value);
            _selectedChildStateMachines.RemoveAll(ele => ele == null);
            CreateSelectedStateMachineNodes();
        }

        private void SelectStateMachineNode(StateMachineNode stateMachineNode)
        {
            _selectedChildStateMachines.Add(stateMachineNode.Value);
            CreateSelectedStateMachineNodes();
        }

        private void GoToMachineNode(StateMachineNode stateMachineNode)
        {
            int index = _selectedChildStateMachines.IndexOf(stateMachineNode.Value);
            _selectedChildStateMachines.RemoveRange(index + 1, (_selectedChildStateMachines.Count - index) - 1);
            CreateSelectedStateMachineNodes();

        }

        private void RemoveDefaultStateModelNode(StateNode node)
        {
            if (IsGraphReadOnly)
                return;

            _runtimeStateMachineController.RemoveDefaultState(SelectedStateMachineModel, node.Value);
            _nodes.Remove(node);

            foreach (var sNode in _nodes)
            {
                if (sNode is StateNode stateNode)
                {
                    SetAsDefaultNode(stateNode, false);
                    break;
                }
            }
        }

        private void RemoveStateModelNode(StateNode node)
        {
            if (IsGraphReadOnly)
                return;

            _runtimeStateMachineController.RemoveState(SelectedStateMachineModel, node.Value);
            _nodes.Remove(node);
            CreateSelectedStateMachineNodes();
        }

        private void SetAsDefaultNode(StateNode stateModelNode, bool isFocused)
        {
            if (IsGraphReadOnly)
                return;

            if (_runtimeStateMachineController.IsAssetBacked())
                _runtimeStateMachineController.SetDefaultNode(stateModelNode.Value);
            stateModelNode.SetDefault(true);
            UpdateDefaultNode();
            stateModelNode.IsFocused = isFocused;
            if (_runtimeStateMachineController.IsAssetBacked())
                EditorUtility.SetDirty(_runtimeStateMachineController);
        }

        private void DuplicateNode(StateNode stateModelNode)
        {
            if (IsGraphReadOnly)
                return;

            var stateModel = stateModelNode.Value.Clone(_runtimeStateMachineController, SelectedStateMachineModel);
            CreateStateModelNode(stateModel);
        }

        private void UpdateDefaultNode()
        {
            foreach (var node in _nodes)
            {
                if (node is StateMachineNode stateMachineNode)
                    stateMachineNode.SetDefault(_runtimeStateMachineController.IsDefaultStateMachine(stateMachineNode.Value));
                else if (node is StateNode stateNode)
                    stateNode.SetDefault(stateNode.Value == _runtimeStateMachineController.GetDefaultState());
            }
        }

        private void SetCurrentActiveNode()
        {
            if (!Application.isPlaying)
                return;

            Repaint();

            if (Actor == null)
                return;

            var stateMachineModel = _runtimeStateMachineController.GetStateMachineModel(Actor?.CurrentState);
            StateMachineNode currentStateMachineNode = null;
            foreach (var node in _nodes)
            {
                if (node is StateMachineNode stateMachineNode)
                {
                    stateMachineNode.IsFocused = stateMachineNode.Value == stateMachineModel;
                    if (stateMachineNode.IsFocused)
                        currentStateMachineNode = stateMachineNode;
                }
            }

            if (stateMachineModel != SelectedStateMachineModel)
            {
                if (currentStateMachineNode != null)
                {
                    if (!stateMachineModel.IsParentOf(SelectedStateMachineModel))
                        SelectStateMachineNode(currentStateMachineNode);
                    else
                        GoToMachineNode(currentStateMachineNode);
                }
            }

            foreach (var node in _nodes)
            {
                if (node is StateNode stateNode)
                    stateNode.IsFocused = stateNode.Value?.State.Name == Actor?.CurrentState.Name;
            }

        }
    }
}
