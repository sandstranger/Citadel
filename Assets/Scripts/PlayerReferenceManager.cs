using System.Collections;
using System.Collections.Generic;
using System.Text;
using Citadel.Game;
using Citadel.SceneManagement;
using Zenject;
using UnityEngine;

public class PlayerReferenceManager : SingletonHelper<PlayerReferenceManager> {
	// External references, required
	public GameObject playerCapsule;
	public GameObject playerCapsuleHardwareLantern;
	public GameObject playerCapsuleHardwareEReader;
	public GameObject playerCapsuleMainCamera;
	public GameObject playerInventory;
	public GameObject playerCanvas;
	public GameObject playerCursor;
	public GameObject playerStatusBar;
	public GameObject playerTextWarningManager;
	public GameObject playerDeathRessurectEffect;
	public GameObject playerDeathEffect;
	public GameObject playerRadiationTreatmentFlash;
	public GameObject playerMFDManager;

	[SerializeField] 
	private PlayerHealth _playerHealth;
	[SerializeField]
	private HealthManager _healthManager;
	[Inject]
	private BiomonitorGraphSystem _biomonitorGraphSystem;
	[Inject] private Const _consts;
	
	public PlayerHealth PlayerHealth => _playerHealth;
	public HealthManager HealthManager => _healthManager;
	
	public static string SavePlayerData(GameObject plyr, PrefabIdentifier prefID) {
		PlayerReferenceManager PRman = plyr.GetComponent<PlayerReferenceManager>();
        StringBuilder s1 = new StringBuilder(200);
        s1.Clear();
		s1.Append("Hacker");//s1.Append(_consts.playerName);
        s1.Append(Utils.splitChar); s1.Append(PlayerHealth.Save(PRman.playerCapsule));
        s1.Append(Utils.splitChar); s1.Append(PlayerEnergy.Save(PRman.playerCapsule));
        s1.Append(Utils.splitChar); s1.Append(PlayerMovement.Save(PRman.playerCapsule));
        s1.Append(Utils.splitChar); s1.Append(PlayerPatch.Save(PRman.playerCapsule));
        s1.Append(Utils.splitChar); s1.Append(MouseLookScript.Save(PRman.playerCapsuleMainCamera));
        s1.Append(Utils.splitChar); s1.Append(HealthManager.Save(PRman.playerCapsule,prefID));
        s1.Append(Utils.splitChar); s1.Append(GUIState.Save(PRman.playerCanvas));
        s1.Append(Utils.splitChar); s1.Append(Inventory.Save(PRman.playerInventory));
        s1.Append(Utils.splitChar); s1.Append(WeaponCurrent.Save(PRman.playerInventory));
        s1.Append(Utils.splitChar); s1.Append(WeaponFire.Save(PRman.playerCapsuleMainCamera));
        s1.Append(Utils.splitChar); s1.Append(MFDManager.Save(PRman.playerMFDManager));
        s1.Append(Utils.splitChar); s1.Append(Automap.Save(PRman.playerMFDManager));
		return s1.ToString();
	}

	public static int LoadPlayerDataToPlayer(Const @const,GameObject currentPlayer,
											 ref string[] entries,int index,
						   					 PrefabIdentifier prefID, int levID) {
		
		PlayerReferenceManager PRman = currentPlayer.GetComponent<PlayerReferenceManager>();
		@const.playerName = entries[index]; index++;
		index = PlayerHealth.Load(PRman.playerCapsule,ref entries,index);
		index = PlayerEnergy.Load(PRman.playerCapsule,ref entries,index);
		index = PlayerMovement.Load(@const,PRman.playerCapsule,ref entries,index);
		index = PlayerPatch.Load(PRman.playerCapsule,ref entries,index);
		index = MouseLookScript.Load(PRman.playerCapsuleMainCamera,ref entries,index);
		index = HealthManager.Load(PRman.playerCapsule,ref entries,index,prefID,levID);
		index = GUIState.Load(PRman.playerCanvas,ref entries,index);
		index = Inventory.Load(@const,PRman.playerInventory,ref entries,index);
		index = WeaponCurrent.Load(PRman.playerInventory,ref entries,index);
		index = WeaponFire.Load(PRman.playerCapsuleMainCamera,ref entries,index);
		index = MFDManager.Load(PRman.playerMFDManager,ref entries,index);
		index = Automap.Load(PRman.playerMFDManager,ref entries,index);
		PRman._biomonitorGraphSystem.ClearGraphs();
		return index;
	}
}
