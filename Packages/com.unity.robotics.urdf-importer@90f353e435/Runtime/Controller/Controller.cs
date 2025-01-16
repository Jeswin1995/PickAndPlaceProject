using System;
using Unity.Robotics;
using UnityEngine;

namespace Unity.Robotics.UrdfImporter.Control
{
    public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };
    public enum ControlType { PositionControl };

    public class Controller : MonoBehaviour
    {
        private ArticulationBody[] articulationChain;
        private Renderer[] previousRenderers;

        [InspectorReadOnly(hideInEditMode: true)]
        public string selectedJoint;
        [HideInInspector]
        public int selectedIndex;

        public ControlType control = ControlType.PositionControl;
        public float stiffness;
        public float damping;
        public float forceLimit;
        public float speed = 5f;
        public float torque = 100f;
        public float acceleration = 5f;

        [Tooltip("Material to highlight the currently selected joint")]
        public Material highlightMaterial;

        void Start()
        {
            previousRenderers = null;
            selectedIndex = 1;
            this.gameObject.AddComponent<FKRobot>();
            articulationChain = this.GetComponentsInChildren<ArticulationBody>();
            int defDynamicVal = 10;

            foreach (ArticulationBody joint in articulationChain)
            {
                joint.gameObject.AddComponent<JointControl>();
                joint.jointFriction = defDynamicVal;
                joint.angularDamping = defDynamicVal;
                ArticulationDrive currentDrive = joint.xDrive;
                currentDrive.forceLimit = forceLimit;
                joint.xDrive = currentDrive;
            }

            HighlightJoint(selectedIndex);
        }

        void Update()
        {
            bool selectionInputNext = Input.GetKeyDown(KeyCode.RightArrow);
            bool selectionInputPrev = Input.GetKeyDown(KeyCode.LeftArrow);

            if (selectionInputPrev)
            {
                SelectPreviousJoint();
            }
            else if (selectionInputNext)
            {
                SelectNextJoint();
            }

            UpdateDirection(selectedIndex);
        }

        private void HighlightJoint(int index)
        {
            // Remove highlight from the previous joint
            if (previousRenderers != null)
            {
                foreach (var renderer in previousRenderers)
                {
                    // Restore the original material
                    if (renderer != null)
                    {
                        renderer.materials = renderer.materials[..^1]; // Remove the last material (highlight)
                    }
                }
            }

            // Highlight the new joint
            if (index >= 0 && index < articulationChain.Length)
            {
                ArticulationBody currentJoint = articulationChain[index];
                previousRenderers = currentJoint.transform.GetChild(0).GetComponentsInChildren<Renderer>();

                foreach (var renderer in previousRenderers)
                {
                    if (renderer != null && highlightMaterial != null)
                    {
                        var originalMaterials = renderer.materials;
                        Array.Resize(ref originalMaterials, originalMaterials.Length + 1);
                        originalMaterials[^1] = highlightMaterial; // Add highlight material
                        renderer.materials = originalMaterials;
                    }
                }

                DisplaySelectedJoint(index);
            }
        }

        void DisplaySelectedJoint(int index)
        {
            if (index >= 0 && index < articulationChain.Length)
            {
                selectedJoint = articulationChain[index].name + " (" + index + ")";
            }
        }

        public void SelectNextJoint()
        {
            selectedIndex = (selectedIndex + 1) % articulationChain.Length;
            HighlightJoint(selectedIndex);
        }

        public void SelectPreviousJoint()
        {
            selectedIndex = (selectedIndex - 1 + articulationChain.Length) % articulationChain.Length;
            HighlightJoint(selectedIndex);
        }

        private void UpdateDirection(int jointIndex)
        {
            if (jointIndex < 0 || jointIndex >= articulationChain.Length) 
            {
                return;
            }

            float moveDirection = Input.GetAxis("Vertical");
            JointControl current = articulationChain[jointIndex].GetComponent<JointControl>();

            if (current.controltype != control)
            {
                UpdateControlType(current);
            }

            if (moveDirection > 0)
            {
                current.direction = RotationDirection.Positive;
            }
            else if (moveDirection < 0)
            {
                current.direction = RotationDirection.Negative;
            }
            else
            {
                current.direction = RotationDirection.None;
            }
        }

        public void UpdateControlType(JointControl joint)
        {
            joint.controltype = control;
            if (control == ControlType.PositionControl)
            {
                ArticulationDrive drive = joint.joint.xDrive;
                drive.stiffness = stiffness;
                drive.damping = damping;
                joint.joint.xDrive = drive;
            }
        }
    }
}
