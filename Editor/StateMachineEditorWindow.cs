using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    [ExecuteInEditMode]
    internal class StateMachineEditorWindow : GridEditorWindow
    {
        protected static RuntimeStateMachineController RuntimeStateMachineController;
        private StateMachineParameterEditor _parameterEditor;
        private SerializedObject _stateMachineModelSO;
        private StateTransitionEditor _transition;

        private List<Node> _nodes = new List<Node>();
        private Actor Actor => Selection.activeGameObject?.GetComponent<Actor>();

        static StateMachineEditorWindow detailsWindow;
        private Stack<string> _selectedChildStateMachines = new Stack<string>();
        private int _selectedIndex = 0;
        private StateMachineModel SelectedStateMachineModel 
        {
          get  {
                return RuntimeStateMachineController?.GetStateMachineModel(_selectedChildStateMachines.Peek());
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
            _nodes.Clear();
            _selectedChildStateMachines.Clear();
            _selectedChildStateMachines.Push(RuntimeStateMachineController.BaseStateMachineModel().name);

            _stateMachineModelSO = new SerializedObject(RuntimeStateMachineController);
            _parameterEditor = new StateMachineParameterEditor(_stateMachineModelSO);
            _transition = new StateTransitionEditor(_stateMachineModelSO);
            CreateSelectedStateMachineNodes();
            /*  var states = _stateMachineModelSO.FindProperty("_stateModels");

              for (int i = 0; i < states.arraySize; ++i)
              {
                  var element = states.GetArrayElementAtIndex(i);
                  var state = element.objectReferenceValue as StateModel;
                  CreateNode(state, new SerializedObject(state).FindProperty("position").vector3Value);
              }*/


            var defaultState = _stateMachineModelSO.FindProperty("_defaultStateModel").objectReferenceValue;
    
            foreach (Node node in _nodes)
            {
                if (node.stateModelSO.targetObject == defaultState)
                    SetAsDefaultNode(node);

                var stateTransitions = node.stateModelSO.FindProperty("m_Transitions");
                for (int i = 0; i < stateTransitions.arraySize; ++i)
                {
                    var element = stateTransitions.GetArrayElementAtIndex(i);
                    var targetState = element.FindPropertyRelative("m_TargetState").objectReferenceValue;
                    Node endNode = _nodes.Find(ele => ele.stateModelSO.targetObject == targetState);
                    if (endNode != null)
                        _transition.Add(node, endNode);
                }
            }

            Repaint();
        }

        private void CreateSelectedStateMachineNodes()
        {
            var stateModels = SelectedStateMachineModel.GetStates();
            for (int i = 0; i < stateModels.Count; ++i)
                CreateStateModelNode(stateModels[i]);
        }


        protected override void OnGUI()
        {
            base.OnGUI();
            if (RuntimeStateMachineController == null)
                return;
            SetCurrentActiveNode();
            _transition.DrawConnectionLine(Event.current);
            _transition.DrawConnections();
            DrawNodes();
            EditorUtilities.HorizontalLine(new Rect(0, 0, position.width, 21), 20, new Color(0.2196079f, 0.2196079f, 0.2196079f));
            DrawStateMachineToolBar(new Rect(0, 0, position.width, 20));
            EditorUtilities.HorizontalLine(new Rect(0, 20, position.width, 21), 1, Color.black);
            ProcessNodeEvents(Event.current);
            ProcessMouseEvent(Event.current);

            if (Application.isPlaying)
                _parameterEditor = new StateMachineParameterEditor(new SerializedObject(RuntimeStateMachineController));
            _parameterEditor.DrawRect(new Rect(0, 1, Mathf.Max(200, position.width / 5), position.height));
            _parameterEditor.DrawParametersWindow();
            EditorUtilities.VerticalLine(new Rect(Mathf.Max(200, position.width / 5) - 2, 1, position.width, position.height), 2, Color.black);

            Repaint();
        }


        private void DrawStateMachineToolBar(Rect rect)
        {
            rect.x = Mathf.Max(200, position.width / 5) - 2;
            var childStateMachines = _selectedChildStateMachines.ToArray();
            _selectedIndex = GUI.Toolbar(rect, _selectedIndex, childStateMachines, Settings.ChildStateMachinestoolBarStyle);
            
            foreach (var name in _selectedChildStateMachines)
            {
                if (!childStateMachines[_selectedIndex].Equals(_selectedChildStateMachines.Peek(), StringComparison.OrdinalIgnoreCase))
                    _selectedChildStateMachines.Pop();
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
                    if (e.button == 0)
                        _transition.ClearConnectionSelection();
                    break;
            }
        }

        private void ProcessNodeEvents(Event e)
        {
            if (_nodes != null)
            {
                for (int i = _nodes.Count - 1; i >= 0; i--)
                {
                    if (_nodes[i].ProcessEvents(e))
                    {
                        Selection.activeObject = _nodes[i].stateModelSO.targetObject;
                        GUI.changed = true;
                    }
                }
            }
        }

        private void DrawNodes()
        {
            foreach (Node node in _nodes)
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
            genericMenu.AddItem(new GUIContent("Create State"), false, () => RuntimeStateMachineController.AddState(SelectedStateMachineModel, "New State", mousePosition));
            genericMenu.ShowAsContext();

            genericMenu.AddItem(new GUIContent("Create Sub-State Machine"), false, () => RuntimeStateMachineController.AddChildStateMachine(SelectedStateMachineModel, "New StateMachine", mousePosition));
            genericMenu.ShowAsContext();
        }

        private void CreateStateModelNode(StateModel state)
        {
            Node node = new Node(state, state.GetPosition(), _transition.StartTransition, _transition.MakeTransion, RemoveNode, SetAsDefaultNode);
            _nodes.Add(node);

            if (_stateMachineModelSO.FindProperty("_stateModels").arraySize == 2)
                SetAsDefaultNode(node);
        }

        private void CreateChildMachinelNode(StateMachineModel stateMachineModel)
        {
           /* Node node = new Node(stateMachineModel, stateMachineModel.GetPosition(), _transition.StartTransition, _transition.MakeTransion, RemoveNode, SetAsDefaultNode);
            _nodes.Add(node);

            if (_stateMachineModelSO.FindProperty("_stateModels").arraySize == 2)
                SetAsDefaultNode(node);*/
        }

        private void RemoveNode(Node node)
        {
            RuntimeStateMachineController.RemoveState(SelectedStateMachineModel, node.state);
           // _transition.ClearNodeConnection(node);
        }

        private void SetAsDefaultNode(Node node)
        {
            var defaultState = _stateMachineModelSO.FindProperty("_defaultStateModel").objectReferenceValue;
            if (defaultState != null)
            {
                Node currentDefaultNode = _nodes.Find(ele => ele.stateModelSO.targetObject == defaultState);
                if (currentDefaultNode != null)
                {
                    currentDefaultNode.isDefault = false;
                    currentDefaultNode.SetActvieStyle(false);
                }
            }

            _stateMachineModelSO.FindProperty("_defaultStateModel").objectReferenceValue = node.stateModelSO.targetObject;
            node.isDefault = true;
            node.SetActvieStyle(true);
        }

        private void SetCurrentActiveNode()
        {
            if (!Application.isPlaying)
                return;
            foreach (Node node in _nodes)
            {
                if (node.stateModelSO.targetObject.name == Actor?.CurrentStateName)
                    node.SetActvieStyle(true);
                else
                    node.SetActvieStyle(false);
            }
        }
    }
}
