using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    [ExecuteInEditMode]
    internal class StateMachineEditorWindow : GridEditorWindow
    {
        protected static StateMachineModel StateMachineModel;
        private StateMachineParameterEditor _parameterEditor;
        private SerializedObject _stateMachineModelSO;
        private StateTransitionEditor _transition;

        private List<Node> _nodes = new List<Node>();
        private HashSet<string> _usedStateNames = new HashSet<string>();
        private Actor Actor => Selection.activeGameObject?.GetComponent<Actor>();

        static StateMachineEditorWindow detailsWindow;

        public static void ShowBehaviourGraphEditor(StateMachineModel target)
        {
            StateMachineModel = target;
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

            var stateMachineModel = Selection.activeObject as StateMachineModel;
            if (stateMachineModel == null)
            {
                if (Actor != null)
                {
                    var actorSO = new SerializedObject(Actor);
                    stateMachineModel = actorSO.FindProperty("m_Controller").objectReferenceValue as StateMachineModel;
                }
            }
            else
                EditorPrefs.SetString("StateMachine", AssetDatabase.GetAssetPath(stateMachineModel));

            if (stateMachineModel != null)
            {
                StateMachineModel = stateMachineModel;
                Initialize();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (StateMachineModel == null)
            {
                if (EditorPrefs.HasKey("StateMachine"))
                    StateMachineModel = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("StateMachine"), typeof(StateMachineModel)) as StateMachineModel;
                if (StateMachineModel == null)
                    return;
            }
            Initialize();
        }

        private void Initialize()
        {
            _nodes.Clear();
            _usedStateNames.Clear();

            _stateMachineModelSO = new SerializedObject(StateMachineModel);
            _parameterEditor = new StateMachineParameterEditor(_stateMachineModelSO);
            _transition = new StateTransitionEditor(_stateMachineModelSO);

            var states = _stateMachineModelSO.FindProperty("_stateModels");

            for (int i = 0; i < states.arraySize; ++i)
            {
                var element = states.GetArrayElementAtIndex(i);
                var state = element.objectReferenceValue as StateModel;
                MakeNode(state, new SerializedObject(state).FindProperty("position").vector3Value);
                _usedStateNames.Add(state.name);
            }

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

        protected override void OnGUI()
        {
            base.OnGUI();
            if (StateMachineModel == null)
                return;
            SetCurrentActiveNode();
            _transition.DrawConnectionLine(Event.current);
            _transition.DrawConnections();
            DrawNodes();
            ProcessNodeEvents(Event.current);
            ProcessMouseEvent(Event.current);

            BeginWindows();
            if (Application.isPlaying)
                _parameterEditor = new StateMachineParameterEditor(new SerializedObject(StateMachineModel));
            _parameterEditor.DrawRect(GUILayout.Window(1, new Rect(0, 1, Mathf.Max(200, position.width / 5), position.height), _parameterEditor.DrawParametersWindow, ""));
            EndWindows();
            
            Repaint();
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
            genericMenu.AddItem(new GUIContent("Create State"), false, () => MakeNode(mousePosition));
            genericMenu.ShowAsContext();
        }

        private void MakeNode(Vector2 mousePosition)
        {
            string title = Util.MakeUniqueName("New State", _usedStateNames);
            MakeNode(AddState(title), mousePosition);
        }

        private void MakeNode(StateModel state, Vector2 mousePosition)
        {
            Node node = new Node(state, mousePosition, _transition.StartTransition, _transition.MakeTransion, RemoveNode, SetAsDefaultNode);
            _nodes.Add(node);

            if (_stateMachineModelSO.FindProperty("_stateModels").arraySize == 2)
                SetAsDefaultNode(node);
        }

        private StateModel AddState(string name)
        {
            var state = CreateInstance<StateModel>();
            state.name = name;
            //state.hideFlags = HideFlags.HideInHierarchy;

            if (StateMachineModel != null)
            {
                AssetDatabase.AddObjectToAsset(state, StateMachineModel);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(StateMachineModel));
            }

            var stateModelList = _stateMachineModelSO.FindProperty("_stateModels");
            stateModelList.InsertArrayElementAtIndex(stateModelList.arraySize);
            var element = stateModelList.GetArrayElementAtIndex(stateModelList.arraySize - 1);
            element.objectReferenceValue = state;
            _stateMachineModelSO.ApplyModifiedProperties();
            return state;
        }

        private void RemoveNode(Node node)
        {
            _transition.ClearNodeConnection(node);

            var states = _stateMachineModelSO.FindProperty("_stateModels");
            for (int i = 0; i < states.arraySize; ++i)
            {
                var element = states.GetArrayElementAtIndex(i);
                var state = element.objectReferenceValue;
                if (state == node.stateModelSO.targetObject)
                {
                    states.DeleteArrayElementAtIndex(i);
                    var lastElement = states.GetArrayElementAtIndex(states.arraySize - 1);
                    states.GetArrayElementAtIndex(i).objectReferenceValue = lastElement.objectReferenceValue;
                    states.arraySize = states.arraySize - 1;
                    states.serializedObject.ApplyModifiedProperties();
                    break;
                }
            }


            _usedStateNames.Remove(node.stateModelSO.targetObject.name);
            _nodes.Remove(node);
            DestroyImmediate(node.stateModelSO.targetObject, true);

            foreach (Node n in _nodes)
            {
                var stateTransitions = n.stateModelSO.FindProperty("m_Transitions");
                for (int i = 0; i < stateTransitions.arraySize; ++i)
                {
                    var element = stateTransitions.GetArrayElementAtIndex(i);

                    if (element.FindPropertyRelative("m_TargetState")?.objectReferenceValue == null)
                        stateTransitions.DeleteArrayElementAtIndex(i);
                }

                stateTransitions.serializedObject.ApplyModifiedProperties();
            }

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(StateMachineModel));
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
