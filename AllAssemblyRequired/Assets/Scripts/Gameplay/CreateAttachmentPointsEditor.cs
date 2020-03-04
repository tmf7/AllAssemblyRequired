#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CreateAttachmentPointsEditor : EditorWindow
{
    private struct StickyBodyInfo
    {
        public StickyBody body;
        public bool isJointsDrawn;
        public bool flipAttachmentPose;

        public StickyJoint[] Joints => body.GetComponentsInChildren<StickyJoint>(true);
    }

    private StickyJoint _attachmentPointPrefab;
    private StickyBodyInfo[] _stickyBodies;
    private Transform _attachmentPoint;
    private Pose _attachmentPose;
    private Pose _counterAttachmentPose;
    private int _numStickyBodies = 0;

    [MenuItem("AAR/Create Attachment Points")]
    private static void CreateWindow()
    {
        GetWindow<CreateAttachmentPointsEditor>("Create Attachment Points");
    }
    
    private void DrawPrefabSelect()
    {
        EditorGUI.BeginChangeCheck();
        _attachmentPointPrefab = (StickyJoint)EditorGUILayout.ObjectField("Attachment Point Prefab", _attachmentPointPrefab, typeof(StickyJoint), false);

        if (EditorGUI.EndChangeCheck() && _attachmentPointPrefab != null)
        {
            if (!PrefabUtility.IsPartOfPrefabAsset(_attachmentPointPrefab) ||
                _attachmentPointPrefab.GetComponentInChildren<StickyJoint>(true) == null)
            {
                EditorUtility.DisplayDialog("Invalid Attachement Prefab", $"The {ObjectNames.NicifyVariableName(nameof(_attachmentPointPrefab))} must be a prefab with a {nameof(StickyJoint)}", "OK");
                _attachmentPointPrefab = null;
            }
        }
    }

    private void AllocateBodiesArray()
    {
        EditorGUI.BeginChangeCheck();
        _numStickyBodies = Mathf.Clamp(EditorGUILayout.IntField("StickyBody Count", _numStickyBodies), 0, int.MaxValue);

        if (EditorGUI.EndChangeCheck())
        {
            if (_numStickyBodies > 0)
            {
                var newArray = new StickyBodyInfo[_numStickyBodies];

                if (_stickyBodies == null)
                {
                    _stickyBodies = newArray;
                }
                else
                {
                    for (int i = 0; i < newArray.Length; ++i)
                    {
                        if (i >= _stickyBodies.Length)
                        {
                            break;
                        }

                        newArray[i] = _stickyBodies[i];
                    }

                    _stickyBodies = newArray;
                }
            }
            else
            {
                _stickyBodies = null; // possibly also iterate the array and null individual references
            }

            if (_attachmentPoint != null)
            {
                _attachmentPoint.position = GetBodyiesCenter();
                _attachmentPoint.rotation = Quaternion.identity;
            }
        }
    }

    private void DrawBodies()
    {
        if (_numStickyBodies == 0)
        {
            return;
        }

        for (int i = 0; i < _stickyBodies.Length; ++i)
        {
            EditorGUILayout.BeginHorizontal();

            _stickyBodies[i].body = (StickyBody)EditorGUILayout.ObjectField(_stickyBodies[i].body?.name ?? "Sticky Body", _stickyBodies[i].body, typeof(StickyBody), true);
            _stickyBodies[i].flipAttachmentPose = EditorGUILayout.Toggle("Flip New Joints", _stickyBodies[i].flipAttachmentPose);

            EditorGUILayout.EndHorizontal();

            DrawJoints(ref _stickyBodies[i]);
        }
    }

    private Vector3 GetBodyiesCenter()
    {
        Vector3 positionSum = Vector3.zero;

        foreach (var stickyBodyInfo in _stickyBodies)
        {
            if (stickyBodyInfo.body != null)
            {
                positionSum += stickyBodyInfo.body.transform.position;
            }
        }

        return positionSum / _stickyBodies.Length;
    }

    private void DrawJoints(ref StickyBodyInfo stickyBodyInfo)
    {
        if (stickyBodyInfo.body == null)
        {
            return;
        }

        stickyBodyInfo.isJointsDrawn = EditorGUILayout.BeginFoldoutHeaderGroup(stickyBodyInfo.isJointsDrawn, $"{nameof(StickyJoint)}s");

        int originalIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = originalIndent + 1;

        if (stickyBodyInfo.isJointsDrawn)
        {
            foreach (var joint in stickyBodyInfo.Joints)
            {
                EditorGUILayout.BeginHorizontal();

                GUI.enabled = false;
                EditorGUILayout.ObjectField(joint, typeof(StickyJoint), true);
                GUI.enabled = true;

                if (GUILayout.Button("Remove"))
                {
                    Undo.DestroyObjectImmediate(joint.gameObject);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUI.indentLevel = originalIndent;
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void InstantiateAttachmentPoint(StickyBodyInfo stickyBodyInfo)
    {
        if (stickyBodyInfo.body != null)
        {
            var attachmentPoint = (StickyJoint)PrefabUtility.InstantiatePrefab(_attachmentPointPrefab, stickyBodyInfo.body.transform);
            Undo.RegisterCreatedObjectUndo(attachmentPoint, "Created GameObject");
            attachmentPoint.transform.position = _attachmentPoint.position;
            attachmentPoint.transform.rotation = !stickyBodyInfo.flipAttachmentPose ? _attachmentPose.rotation : _counterAttachmentPose.rotation;
        }

    }

    private void DrawAttachmentPoint()
    {
        EditorGUI.BeginChangeCheck();

        _attachmentPoint = (Transform)EditorGUILayout.ObjectField("Attachment Point", _attachmentPoint, typeof(Transform), true);

        if (EditorGUI.EndChangeCheck() &&
            _attachmentPoint != null)
        {
            _attachmentPoint.position = GetBodyiesCenter();
            _attachmentPoint.rotation = Quaternion.identity;
        }
    }

    private void UpdateAttachmentPoses()
    {
        if (_attachmentPoint != null)
        {
            _attachmentPose = new Pose(_attachmentPoint.position, _attachmentPoint.rotation);
            _counterAttachmentPose = new Pose(_attachmentPoint.position, Quaternion.LookRotation(-_attachmentPoint.forward, _attachmentPoint.up));
        }
    }

    private void OnGUI()
    {
        Undo.RecordObject(this, "Attachment Editor");

        DrawPrefabSelect();
        DrawAttachmentPoint();
        UpdateAttachmentPoses();
        AllocateBodiesArray();
        DrawBodies();


        if (GUILayout.Button("Create Attachment Points"))
        {
            if (_attachmentPoint != null)
            {
                foreach (var stickyBodyInfo in _stickyBodies)
                {
                    InstantiateAttachmentPoint(stickyBodyInfo);
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Unassigned Attachment point", $"{ObjectNames.NicifyVariableName(nameof(_attachmentPoint))} needs to be assigned to position the new attachment points.", "OK");
            }
        }
    }
}
#endif