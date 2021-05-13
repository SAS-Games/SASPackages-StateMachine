using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace SAS.StateMachineGraph.Editor
{
    [ExecuteInEditMode]
    internal class StateMachineEditorWindow : GridEditorWindow
    {
        protected static RuntimeStateMachineController RuntimeStateMachineController;
        private StateMachineParameterEditor _parameterEditor;
        private SerializedObject _stateMachineModelSO;
        private StateTransitionEditor _transition;

        private List<BaseNode> _nodes = new List<BaseNode>();
        private Actor Actor => Selection.activeGameObject?.GetComponent<Actor>();

        static StateMachineEditorWindow detailsWindow;
        private List<StateMachineModel> _selectedChildStateMachines = new List<StateMachineModel>();
        private StateMachineModel SelectedStateMachineModel
        {
            get
            {
                return _selectedChildStateMachines[_selectedChildStateMachines.Count - 1];
            }
        }

        public static void ShowBehaviourGraphEditor(RuntimeStateMachineController target)
        {
            RuntimeStateMachineController = target;
            detailsWindow = GetWindow<StateMachineEditorWindow>(typeof(SceneView));
            GUIContent content = new GUIContent("StateMachine");
            detailsWindow.titleContent = content;
            detailsWindow.ShowTab();
            detailsWindow.Repaint();
        }

        void OnSelectionChange()
        {
            if (Selection.activeObject == null)
                return;

            var stateMachineModel = Selection.activeObject as RuntimeStateMachineController;
            if (stateMachineModel == null)
            {
                if (Actor != null)
                {
                    var actorSO = new SerializedObject(Actor);
                    stateMachineModel = actorSO.FindProperty("m_Controller").objectReferenceValue as RuntimeStateMachineController;
                }
            }
            else
                EditorPrefs.SetString("StateMachine", AssetDatabase.GetAssetPath(stateMachineModel));

            if (stateMachineModel != null)
            {
                RuntimeStateMachineController = stateMachineModel;
                Initialize();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (RuntimeStateMachineController == null)
            {
                if (EditorPrefs.HasKey("StateMachine"))
                    RuntimeStateMachineController = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("StateMachine"), typeof(RuntimeStateMachineController)) as RuntimeStateMachineController;
                if (RuntimeStateMachineController == null)
                    return;
            }

            Initialize();
        }

        private void Initialize()
        {
            _selectedChildStateMachines.Clear();
            _selectedChildStateMachines.Add(RuntimeStateMachineController.BaseStateMachineModel());

            _stateMachineModelSO = new SerializedObject(RuntimeStateMachineController);
            _parameterEditor = new StateMachineParameterEditor(_stateMachineModelSO);
            _transition = new StateTransitionEditor(_stateMachineModelSO);

            CreateSelectedStateMachineNodes();

            /* foreach (BaseNode node in _nodes)
             {
                 var stateTransitions = node.serializedObject.FindProperty("m_Transitions");
                 for (int i = 0; i < stateTransitions.arraySize; ++i)
                 {
                     var element = stateTransitions.GetArrayElementAtIndex(i);
                     var targetState = element.FindPropertyRelative("m_TargetState").objectReferenceValue;
                     BaseNode endNode = _nodes.Find(ele => ele.serializedObject.targetObject == targetState);
                     if (endNode != null)
                         _transition.Add(node, endNode);
                 }
             }*/

            Repaint();
        }

        private void CreateSelectedStateMachineNodes()
        {
            _nodes.Clear();

            var anyStateNode = new AnyStateNode(SelectedStateMachineModel, RuntimeStateMachineController.AnyStateModelSO(), SelectedStateMachineModel.GetAnyStatePosition(), null);
            _nodes.Add(anyStateNode);

            var stateMachineModels = SelectedStateMachineModel.GetChildStateMachines();
            for (int i = 0; i < stateMachineModels.Count; ++i)
                CreateChildMachinelNode(stateMachineModels[i]);

            var stateModels = SelectedStateMachineModel.GetStates();
            for (int i = 0; i < stateModels.Count; ++i)
                CreateStateModelNode(stateModels[i]);
        }


        protected override void OnGUI()
        {
            base.OnGUI();

            DrawStateMachineWindow();
            DrawParameterWindow();
            EditorUtilities.VerticalLine(new Rect(Mathf.Max(200, position.width / 5) - 2, 1, position.width, position.height), 2, Color.black);

            Repaint();
        }

        private void DrawStateMachineWindow()
        {
            if (RuntimeStateMachineController == null)
                return;
            // SetCurrentActiveNode();

            // _transition.DrawConnectionLine(Event.current);
            //  _transition.DrawConnections();
            DrawNodes();
            EditorUtilities.HorizontalLine(new Rect(0, 0, position.width, 21), 20, new Color(0.2196079f, 0.2196079f, 0.2196079f));
            DrawStateMachineToolBar(new Rect(0, 0, position.width, 20));
            EditorUtilities.HorizontalLine(new Rect(0, 20, position.width, 21), 1, Color.black);
            ProcessNodeEvents(Event.current);
            ProcessMouseEvent(Event.current);
        }

        private void DrawParameterWindow()
        {
            BeginWindows();

            if (Application.isPlaying && RuntimeStateMachineController != null)
                _parameterEditor = new StateMachineParameterEditor(new SerializedObject(RuntimeStateMachineController));

            var windowRect = GUI.Window(1, new Rect(0, 0, Mathf.Max(200, position.width / 5), position.height), _parameterEditor.DrawParametersWindow, "");
            GUI.UnfocusWindow();
            _parameterEditor.DrawRect(windowRect);
            EndWindows();
        }

        int selectedIndex = 0;
        private void DrawStateMachineToolBar(Rect rect)
        {
            rect.x = Mathf.Max(200, position.width / 5) - 2;
            var childStateMachines = _selectedChildStateMachines.Select(ele => ele.name).ToArray();

            selectedIndex = GUI.Toolbar(rect, _selectedChildStateMachines.Count - 1, childStateMachines, Settings.ChildStateMachinestoolBarStyle);

            if (selectedIndex != _selectedChildStateMachines.Count - 1)
            {
                _selectedChildStateMachines.RemoveRange(selectedIndex + 1, (_selectedChildStateMachines.Count - selectedIndex) - 1);
                CreateSelectedStateMachineNodes();
            }
        }

        protected override void ProcessMouseEvent(Event e)
        {
            base.ProcessMouseEvent(e);
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 1)
                        ProcessContextMenu(e.mousePosition);
                    break;
                case EventType.MouseUp:
                    // if (e.button == 0)
                    //     _transition.ClearConnectionSelection();
                    break;
            }
        }

        private void ProcessNodeEvents(Event e)
        {
            if (_nodes != null)
            {
                for (int i = _nodes.Count - 1; i >= 0 && i < _nodes.Count; i--)
                {
                    if (_nodes[i].ProcessEvents(e))
                    {
                        Selection.activeObject = _nodes[i].serializedObject.targetObject;
                        GUI.changed = true;
                    }
                }
            }
        }

        private void DrawNodes()
        {
            foreach (BaseNode node in _nodes)
                node.Draw();
        }

        protected override void OnDrag(Vector2 delta)
        {
            base.OnDrag(delta);
            if (_nodes != null)
            {
                for (int i = 0; i < _nodes.Count; i++)
                    _nodes[i].Drag(delta);
            }
            GUI.changed = true;
        }

        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Create State"), false, () => AddState(mousePosition));
            genericMenu.ShowAsContext();

            genericMenu.AddItem(new GUIContent("Create Sub-State Machine"), false, () => AddChildStateMachine(mousePosition));
            genericMenu.ShowAsContext();
        }

        private void AddState(Vector2 mousePosition)
        {
            var stateModel = RuntimeStateMachineController.AddState(SelectedStateMachineModel, "New State", mousePosition);
            CreateStateModelNode(stateModel);
        }

        private void AddChildStateMachine(Vector2 mousePosition)
        {
            var stateMachineModel = RuntimeStateMachineController.AddChildStateMachine(SelectedStateMachineModel, "New StateMachine", mousePosition);
            CreateChildMachinelNode(stateMachineModel);
        }

        private void CreateStateModelNode(StateModel stateModel)
        {
            StateNode node;
            if (stateModel == RuntimeStateMachineController.GetDefaultState() || RuntimeStateMachineController.GetAllStateModels().Count == 1)
            {
                node = new DefaultStateNode(new SerializedObject(stateModel), stateModel.GetPosition(), _transition.StartTransition, _transition.MakeTransion, RemoveDefaultStateModelNode);
                SetAsDefaultNode(node, false);
            }
            else
                node = new StateNode(new SerializedObject(stateModel), stateModel.GetPosition(), _transition.StartTransition, _transition.MakeTransion, RemoveStateModelNode, SetAsDefaultNode);

            _nodes.Add(node);
        }

        private void CreateChildMachinelNode(StateMachineModel stateMachineModel)
        {
            var node = new StateMachineNode(new SerializedObject(stateMachineModel), stateMachineModel.GetPosition(), _transition.StartTransition, _transition.MakeTransion, RemoveStateMachineNode, SelectStateMachineNode);
            _nodes.Add(node);
        }

        private void RemoveStateMachineNode(StateMachineNode stateMachineNode)
        {
            RuntimeStateMachineController.RemoveStateMachine(stateMachineNode.Value);
            _selectedChildStateMachines.RemoveAll(ele => ele == null);
            CreateSelectedStateMachineNodes();
        }

        private void SelectStateMachineNode(StateMachineNode stateMachineNode)
        {
            _selectedChildStateMachines.Add(stateMachineNode.Value);
            CreateSelectedStateMachineNodes();
        }

        private void RemoveDefaultStateModelNode(StateNode node)
        {
            RuntimeStateMachineController.RemoveDefaultState(SelectedStateMachineModel, node.Value);
            RemoveStateModelNode(node);
        }

        private void RemoveStateModelNode(StateNode node)
        {
            RuntimeStateMachineController.RemoveState(SelectedStateMachineModel, node.Value);
            _nodes.Remove(node);
            // _transition.ClearNodeConnection(node);
        }

        private void SetAsDefaultNode(StateNode stateModelNode, bool isFocused)
        {
            RuntimeStateMachineController.SetDefaultNode(stateModelNode.Value);
            stateModelNode.IsFocused = isFocused;
            EditorUtility.SetDirty(RuntimeStateMachineController);
        }

        private void SetCurrentActiveNode()
        {
            if (!Application.isPlaying)
                return;
            foreach (BaseNode node in _nodes)
            {
                if (node.serializedObject.targetObject.name == Actor?.CurrentStateName)
                    node.IsFocused = true;
                else
                    node.IsFocused = false;
            }
        }
    }
}
