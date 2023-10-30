using System.Collections;
using System.Collections.Generic;
using HyperCasual.Core;
using UnityEngine;
using TMPro;


namespace HyperCasual.Runner
{
    /// <summary>
    /// This View contains main menu functionalities
    /// </summary>
    public class MainMenu : View
    {
        [SerializeField]
        HyperCasualButton m_StartButton;
        [SerializeField]
        HyperCasualButton m_SettingsButton;
        [SerializeField]
        HyperCasualButton m_ShopButton;
        [SerializeField]
        TextMeshProUGUI m_LevelText;
        [SerializeField]
        AbstractGameEvent m_StartButtonEvent;


        int m_LevelValue = -1;

        /// <summary>
        /// The amount of gold to display on the celebration screen.
        /// The setter method also sets the celebration screen text.
        /// </summary>
        public int LevelValue
        {
            get => m_LevelValue;
            set
            {
                if (m_LevelValue != value)
                {
                    m_LevelValue = value;
                    m_LevelText.text = "Level " + (m_LevelValue +1).ToString();
                }
            }
        }
        void OnEnable()
        {
            m_StartButton.AddListener(OnStartButtonClick);
            m_SettingsButton.AddListener(OnSettingsButtonClick);
            m_ShopButton.AddListener(OnShopButtonClick);
        }
        
        void OnDisable()
        {
            m_StartButton.RemoveListener(OnStartButtonClick);
            m_SettingsButton.RemoveListener(OnSettingsButtonClick);
            m_ShopButton.RemoveListener(OnShopButtonClick);
        }

        void OnStartButtonClick()
        {
            m_StartButtonEvent.Raise();
            AudioManager.Instance.PlayEffect(SoundID.ButtonSound);
        }

        void OnSettingsButtonClick()
        {
            UIManager.Instance.Show<SettingsMenu>();
            AudioManager.Instance.PlayEffect(SoundID.ButtonSound);
        }

        void OnShopButtonClick()
        {
            UIManager.Instance.Show<ShopView>();
            AudioManager.Instance.PlayEffect(SoundID.ButtonSound);
        }
    }
}