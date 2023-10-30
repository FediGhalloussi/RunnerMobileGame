using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HyperCasual.Runner
{
    /// <summary>
    /// A class used to control a player in a Runner
    /// game. Includes logic for player movement as well as 
    /// other gameplay logic.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /// <summary> Returns the PlayerController. </summary>
        public static PlayerController Instance => s_Instance;
        static PlayerController s_Instance;

        [SerializeField]
        GameObject m_WaterContainer;

        [SerializeField]
        Animator m_Animator;

        [SerializeField]
        SkinnedMeshRenderer m_SkinnedMeshRenderer;

        [SerializeField]
        PlayerSpeedPreset m_PlayerSpeed = PlayerSpeedPreset.Medium;

        [SerializeField]
        float m_CustomPlayerSpeed = 10.0f;

        [SerializeField]
        float m_AccelerationSpeed = 10.0f;

        [SerializeField]
        float m_DecelerationSpeed = 20.0f;

        [SerializeField]
        float m_HorizontalSpeedFactor = 0.5f;

        [SerializeField]
        float m_ScaleVelocity = 2.0f;
        [SerializeField]
        float m_WaterScaleVelocity = 2.0f;
        [SerializeField]
        float m_DividerWaterIncrement = 50f;

        [SerializeField]
        bool m_AutoMoveForward = true;

        Vector3 m_LastPosition;
        float m_StartHeight;
        bool m_CanMove = false;
        bool m_IsThrowing = false;

        /// <summary>
        /// Gets or sets whether the player can move.
        /// </summary>
        public bool CanMove
        {
            get { return m_CanMove; }
            set { m_CanMove = value; }
        }

        const float k_MinimumScale = .5f;
        static readonly string s_Speed = "Speed";
        static readonly string s_HasArrived = "HasArrived";

        enum PlayerSpeedPreset
        {
            Slow,
            Medium,
            Fast,
            Custom
        }

        Transform m_Transform;
        Vector3 m_StartPosition;
        bool m_HasInput;
        float m_MaxXPosition;
        float m_XPos;
        float m_ZPos;
        float m_TargetPosition;
        float m_Speed;
        float m_TargetSpeed;
        Vector3 m_Scale;
        Vector3 m_TargetScale;
        Vector3 m_WaterScale;
        Vector3 m_WaterTargetScale;
        Vector3 m_DefaultScale;
        Vector3 m_WaterDefaultScale;

        const float k_HalfWidth = 0.5f;

        /// <summary> The player's root Transform component. </summary>
        public Transform Transform => m_Transform;

        /// <summary> The player's current speed. </summary>
        public float Speed => m_Speed;

        /// <summary> The player's target speed. </summary>
        public float TargetSpeed => m_TargetSpeed;

        /// <summary> The player's minimum possible local scale. </summary>
        public float MinimumScale => k_MinimumScale;

        /// <summary> The player's current local scale. </summary>
        public Vector3 Scale => m_Scale;

        /// <summary> The player's target local scale. </summary>
        public Vector3 TargetScale => m_TargetScale;

        /// <summary> The player's default local scale. </summary>
        public Vector3 DefaultScale => m_DefaultScale;

        /// <summary> The player's default local height. </summary>
        public float StartHeight => m_StartHeight;

        /// <summary> The player's default local height. </summary>
        public float TargetPosition => m_TargetPosition;

        /// <summary> The player's maximum X position. </summary>
        public float MaxXPosition => m_MaxXPosition;

        void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;

            Initialize();
        }

        /// <summary>
        /// Set up all necessary values for the PlayerController.
        /// </summary>
        public void Initialize()
        {
            m_Transform = transform;
            m_StartPosition = m_Transform.position;
            m_DefaultScale = m_Transform.localScale;
            m_Scale = m_DefaultScale;
            m_TargetScale = m_Scale;
            m_WaterDefaultScale = m_WaterContainer.transform.localScale;
            m_WaterScale = m_WaterDefaultScale;
            m_WaterTargetScale = m_WaterScale;

            if (m_SkinnedMeshRenderer != null)
            {
                m_StartHeight = m_SkinnedMeshRenderer.bounds.size.y;
            }
            else 
            {
                m_StartHeight = 1.0f;
            }

            ResetSpeed();
        }

        /// <summary>
        /// Returns the current default speed based on the currently
        /// selected PlayerSpeed preset.
        /// </summary>
        public float GetDefaultSpeed()
        {
            switch (m_PlayerSpeed)
            {
                case PlayerSpeedPreset.Slow:
                    return 5.0f;

                case PlayerSpeedPreset.Medium:
                    return 10.0f;

                case PlayerSpeedPreset.Fast:
                    return 20.0f;
            }

            return m_CustomPlayerSpeed;
        }

        /// <summary>
        /// Adjust the player's current speed
        /// </summary>
        public void AdjustSpeed(float speed)
        {
            m_TargetSpeed += speed;
            m_TargetSpeed = Mathf.Max(0.0f, m_TargetSpeed);
        }

        /// <summary>
        /// Reset the player's current speed to their default speed
        /// </summary>
        public void ResetSpeed()
        {
            m_Speed = 0.0f;
            m_TargetSpeed = GetDefaultSpeed();
        }

        /// <summary>
        /// Adjust the player's current scale
        /// </summary>
        public void AdjustScale(float scale)
        {
            m_TargetScale += Vector3.one * scale;
            m_TargetScale = Vector3.Max(m_TargetScale, Vector3.one * k_MinimumScale);
        }

        /// <summary>
        /// Reset the player's current speed to their default speed
        /// </summary>
        public void ResetScale()
        {
            m_Scale = m_DefaultScale;
            m_TargetScale = m_DefaultScale;
        }

        /// <summary>
        /// Adjust the player's current scale
        /// </summary>
        public void AdjustWater(float water)
        {
            m_WaterTargetScale += Vector3.one * (water/m_DividerWaterIncrement);
            m_WaterTargetScale = Vector3.Max(m_WaterTargetScale, Vector3.one * k_MinimumScale);
        }

        /// <summary>
        /// Reset the player's current speed to their default speed
        /// </summary>
        public void ResetWater()
        {
            m_WaterScale = m_WaterDefaultScale;
            m_WaterTargetScale = m_WaterDefaultScale;
        }

        /// <summary>
        /// Returns the player's transform component
        /// </summary>
        public Vector3 GetPlayerTop()
        {
            return m_Transform.position + Vector3.up * (m_StartHeight * m_Scale.y - m_StartHeight);
        }

        /// <summary>
        /// Sets the target X position of the player
        /// </summary>
        public void SetDeltaPosition(float normalizedDeltaPosition)
        {
            if (m_MaxXPosition == 0.0f)
            {
                Debug.LogError("Player cannot move because SetMaxXPosition has never been called or Level Width is 0. If you are in the LevelEditor scene, ensure a level has been loaded in the LevelEditor Window!");
            }

            float fullWidth = m_MaxXPosition * 2.0f;
            m_TargetPosition = m_TargetPosition + fullWidth * normalizedDeltaPosition;
            m_TargetPosition = Mathf.Clamp(m_TargetPosition, -m_MaxXPosition, m_MaxXPosition);
            m_HasInput = true;
        }

        /// <summary>
        /// Stops player movement
        /// </summary>
        public void CancelMovement()
        {
            m_HasInput = false;

        }
        /// <summary>
        /// Stops player movement
        /// </summary>
        public void CancelAllMovement()
        {
            m_HasInput = false;
            m_CanMove = false;
            ResetSpeed();

        }

        public void LaunchThrowAnimation()
        {
            if (m_Animator != null)
            {
                m_IsThrowing = true;
                m_Animator.SetTrigger(s_HasArrived);
            }
            
            // Calculate the direction towards the target
            Vector3 direction = FindObjectOfType<BossController>().gameObject.transform.position - transform.position;
            // direction.x += 1;
            // direction.z -= 1;
            
            m_Transform.forward = direction;
        }
        
        public IEnumerator WaitAndEndGame()
        {
            bool isBossCleaned = GameManager.Instance.CleanBoss();
            yield return new WaitForSeconds(3.0f); // Wait for 3 seconds

            if (GameManager.Instance != null)
            {
                if (isBossCleaned)
                    GameManager.Instance.Win();
                else
                    GameManager.Instance.Lose();
            }
        }
        private void ThrowObject()
        {
            // Detach the object from the hand
            m_WaterContainer.transform.parent = null;

            // Add a Rigidbody if not already present
            Rigidbody rb = m_WaterContainer.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = m_WaterContainer.AddComponent<Rigidbody>();
            }

            // Calculate the direction towards the target
            Vector3 direction = FindObjectOfType<BossController>().gameObject.transform.position - transform.position;
            direction.x += 1;
            direction.z -= 1;
            // Calculate the initial velocity for a parabolic trajectory
            float timeToTarget = direction.magnitude / 8.0f; // Adjust the divisor to control the trajectory
            Vector3 initialVelocity = direction / timeToTarget - Physics.gravity * timeToTarget / 2;

            // Set the initial velocity to simulate the throw
            rb.velocity = initialVelocity;
        }

        // This method will be called by the Animation Event at the end of the animation
        public void OnThrowAnimationEnd()
        {
            ThrowObject();
            m_IsThrowing = false;
        }

        /// <summary>
        /// Set the level width to keep the player constrained
        /// </summary>
        public void SetMaxXPosition(float levelWidth)
        {
            // Level is centered at X = 0, so the maximum player
            // X position is half of the level width
            m_MaxXPosition = levelWidth * k_HalfWidth;
        }

        /// <summary>
        /// Returns player to their starting position
        /// </summary>
        public void ResetPlayer()
        {
            m_Transform.position = m_StartPosition;
            m_XPos = 0.0f;
            m_ZPos = m_StartPosition.z;
            m_TargetPosition = 0.0f;

            m_LastPosition = m_Transform.position;

            m_HasInput = false;

            ResetSpeed();
            ResetScale();
        }

        /// <summary>
        /// Adjust the player's current scale
        /// </summary>
        public float GetWaterLevel()
        {
            return (m_WaterScale.x - m_WaterDefaultScale.x) * m_DividerWaterIncrement;
        }
        void Update()
        {
            float deltaTime = Time.deltaTime;

            // Update Scale

            if (!Approximately(m_Transform.localScale, m_TargetScale))
            {
                m_Scale = Vector3.Lerp(m_Scale, m_TargetScale, deltaTime * m_ScaleVelocity);
                m_Transform.localScale = m_Scale;
            }

            // Update Water Scale

            if (!Approximately(m_WaterContainer.transform.localScale, m_WaterTargetScale))
            {
                m_WaterScale = Vector3.Lerp(m_WaterScale, m_WaterTargetScale, deltaTime * m_WaterScaleVelocity);
                m_WaterContainer.transform.localScale = m_WaterScale;
            }

            // Update Speed

            if ((!m_AutoMoveForward && !m_HasInput) || !CanMove)
            {
                Decelerate(deltaTime, 0.0f);
            }
            else if (m_TargetSpeed < m_Speed)
            {
                Decelerate(deltaTime, m_TargetSpeed);
            }
            else if (m_TargetSpeed > m_Speed)
            {
                Accelerate(deltaTime, m_TargetSpeed);
            }

            float speed = m_Speed * deltaTime;

            // Update position

            m_ZPos += speed;

            if (m_HasInput)
            {
                float horizontalSpeed = speed * m_HorizontalSpeedFactor;

                float newPositionTarget = Mathf.Lerp(m_XPos, m_TargetPosition, horizontalSpeed);
                float newPositionDifference = newPositionTarget - m_XPos;

                newPositionDifference = Mathf.Clamp(newPositionDifference, -horizontalSpeed, horizontalSpeed);

                m_XPos += newPositionDifference;
            }

            m_Transform.position = new Vector3(m_XPos, m_Transform.position.y, m_ZPos);

            if (m_Animator != null && deltaTime > 0.0f)
            {
                float distanceTravelledSinceLastFrame = (m_Transform.position - m_LastPosition).magnitude;
                float distancePerSecond = distanceTravelledSinceLastFrame / deltaTime;

                m_Animator.SetFloat(s_Speed, distancePerSecond);
            }

            if (m_Transform.position != m_LastPosition)
            {
                m_Transform.forward = Vector3.Lerp(m_Transform.forward, (m_Transform.position - m_LastPosition).normalized, speed);
            }

            m_LastPosition = m_Transform.position;
        }

        void Accelerate(float deltaTime, float targetSpeed)
        {
            m_Speed += deltaTime * m_AccelerationSpeed;
            m_Speed = Mathf.Min(m_Speed, targetSpeed);
        }

        void Decelerate(float deltaTime, float targetSpeed)
        {
            m_Speed -= deltaTime * m_DecelerationSpeed;
            m_Speed = Mathf.Max(m_Speed, targetSpeed);
        }

        bool Approximately(Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
        }
    }
}