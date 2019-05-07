using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;
using System.Collections.Generic;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;

            if (Input.GetKeyDown(KeyCode.K))
            {
                Respawn();
            }
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x*speed;
            m_MoveDir.z = desiredMove.z*speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            m_MouseLook.UpdateCursorLock();
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            m_MouseLook.LookRotation (transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }


        private void Respawn()
        {
            int[] size = GameManager.instance.GetMazeSize();
            byte[,] mazeArray = Generator.mazeArray;

            // The below numbers use (size[n] - 2) / 2 because 
            //     - the minues 2 part is for the walls on each side 
            //       of the maze.
            //     - the divided 2 part is because the origin is at (0, 0)
            //       and the size is split on each side.
            //int new_x = Random.Range(-(size[0] - 2) / 2, (size[0] - 2) / 2);
            //int new_z = Random.Range(-(size[1] - 2) / 2, (size[1] - 2) / 2);
            float new_y = transform.position.y;
            Tuple<int, int> temp = new Tuple<int, int>(0, 0);

            // align is used to figure out which side the player will be
            // respawned at.
            int align = Random.Range(0, 4);
            switch (align)
            {
                // align == 0: player respawns on left side
                case 0:
                    List<Tuple<int, int>> empty_cells = FindEmptyCells(false, true);
                    int index = Random.Range(0, empty_cells.Count);
                    Debug.Log(empty_cells + "; " + index);
                    temp = empty_cells[index];

                    //while (mazeArray[0, new_z + (size[1] - 2) / 2] != 0)
                    //{
                    //    Debug.Log("Case 0: (0, " + (new_z + (size[1] - 2) / 2) + ") = " + mazeArray[0, new_z + (size[1] - 2) / 2]);
                    //    new_z = Random.Range(-(size[1] - 2) / 2, (size[1] - 2) / 2);
                    //}
                    //new_position = new Vector3(temp.Item1, new_y, temp.Item2);
                    break;

                // align == 1: player respawns on right side
                case 1:
                    empty_cells = FindEmptyCells(false, false);
                    index = Random.Range(0, empty_cells.Count);
                    Debug.Log(empty_cells + "; " + index);
                    temp = empty_cells[index];

                    //while (mazeArray[size[1] - 1, new_z + (size[1] - 2) / 2] != 0)
                    //{
                    //    Debug.Log("Case 1: (" + (size[1] - 1) + ", " + (new_z + (size[1] - 2) / 2) + ") = " + mazeArray[(size[1] - 2) / 2, new_z + (size[1] - 2) / 2]);
                    //    new_z = Random.Range(-(size[1] - 2) / 2, (size[1] - 2) / 2);
                    //}
                    //new_position = new Vector3((size[1] - 2) / 2, new_y, new_z);
                    break;

                // align == 2: player respanws on top side
                case 2:
                    empty_cells = FindEmptyCells(true, true);
                    index = Random.Range(0, empty_cells.Count);
                    Debug.Log(empty_cells + "; " + index);
                    temp = empty_cells[index];

                    //while (mazeArray[new_x + (size[0] - 2) / 2, 0] != 0)
                    //{
                    //    Debug.Log("Case 2: (" + (new_x + (size[0] - 2) / 2) + ", 0) = " + mazeArray[new_x + (size[0] - 2) / 2, 0]);
                    //    new_x = Random.Range(-(size[1] - 2) / 2, (size[1] - 2) / 2);
                    //}
                    //Debug.Log("Case 2: (" + (new_x + (size[0] - 2) / 2) + ", 0) = " + mazeArray[new_x + (size[0] - 2) / 2, 0]);
                    //new_position = new Vector3(new_x, new_y, 0);
                    break;

                // align == 3: player respawns on bottom side
                case 3:
                    empty_cells = FindEmptyCells(true, false);
                    index = Random.Range(0, empty_cells.Count);
                    Debug.Log(empty_cells + "; " + index);
                    temp = empty_cells[index];

                    //while (mazeArray[new_x + (size[0] - 2) / 2, size[0] - 1] != 0)
                    //{
                    //    Debug.Log("Case 3: (" + (new_x + (size[0] - 2) / 2) + ", " + (size[0] - 1) + ") = " + mazeArray[new_x + (size[0] - 2) / 2, (size[0] - 2) / 2]);
                    //    new_x = Random.Range(-(size[1] - 2) / 2, (size[1] - 2) / 2);
                    //}
                    //Debug.Log("Case 3: (" + (new_x + (size[0] - 2) / 2) + ", " + (size[0] - 2) / 2 + ") = " + mazeArray[new_x + (size[0] - 2) / 2, (size[0] - 2) / 2]);
                    //new_position = new Vector3(new_x, new_y, (size[0] - 2) / 2);
                    break;
            }

            Vector3 new_position = new Vector3(temp.Item1, new_y, temp.Item2);

            Debug.Log("Align = " + align + "\tNew position = " + new_position);

            // Instantiate new Player at new position
            Instantiate(gameObject, new_position, Quaternion.identity);

            // Replace this body with a monster gameobject
            Destroy(gameObject);
        }

        private List<Tuple<int, int>> FindEmptyCells(bool row_restricted, bool start_restircted)
        {
            int[] size = GameManager.instance.GetMazeSize();
            byte[,] mazeArray = Generator.mazeArray;
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();

            // Searching for empty cells on top or bottom side
            if (row_restricted)
            {
                // Search for empty cells on top side,
                // else search for empty cells on bottom side.
                int index = 1;
                if (!start_restircted)
                {
                    index = size[1] - 2;
                }

                Debug.Log("CEHCKING: " + result.Count + "; " + index);
                while (result.Count == 0 && index >= 1 && index <= size[1] - 2)
                {
                    Debug.Log("HERE");
                    for (int i = 1; i < size[0] - 2; ++i)
                    {
                        if (mazeArray[i, index] == 0)
                        {
                            Debug.Log("Adding coordinates at (" + i + ", " + index + ")");
                            result.Add(new Tuple<int, int>(i - (size[1] - 2) / 2, index - (size[0] - 2) / 2));
                        }
                    }

                    if (start_restircted)
                        ++index;
                    else
                        --index;
                }
            }

            // Otherwise search for empty cells on right or left side
            else
            {
                // Search for empty cells on left side, 
                // else search for empty cells on right side.
                int index = 1;
                if (!start_restircted)
                {
                    index = size[0] - 2;
                }

                Debug.Log("CEHCKING: " + result.Count + "; " + index);
                while (result.Count == 0 && index >= 1 && index <= size[0] - 2)
                {
                    Debug.Log("OR HERE");
                    for (int i = 1; i < size[1] - 2; ++i)
                    {
                        if (mazeArray[index, i] == 0)
                        {
                            Debug.Log("Adding coordinates at (" + index + ", " + i + ")");
                            // Result is adjusted to the wacked up maze generation in scene.
                            result.Add(new Tuple<int, int>(index - (size[1] - 2) / 2, i - (size[0] - 2) / 2));
                        }
                    }

                    if (start_restircted)
                        ++index;
                    else
                        --index;
                }
            }

            foreach (Tuple<int, int> i in result) {
                Debug.Log("(" + i.Item1 + ", " + i.Item2 + ")");
            }
            return result;
        }
    }
}
