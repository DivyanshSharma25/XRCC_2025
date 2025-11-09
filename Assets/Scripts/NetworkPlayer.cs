using UnityEngine;
using Photon.Pun;

/// <summary>
/// Handles Photon ownership for a player prefab and disables local-only components
/// on remote instances (IKTargetFollowVRRig and AnimateOnInput).
/// </summary>
public class NetworkPlayer : MonoBehaviourPun
{
    [Header("Cached Components")]
    public IKTargetFollowVRRig IkTargetFollow { get; private set; }
    public AnimateOnInput[] AnimateOnInputScripts { get; private set; }

    /// <summary>True if this PhotonView belongs to the local player.</summary>
    public bool IsLocalPlayer { get; private set; }

    private void Awake()
    {
        // Cache components (including inactive children)
        IkTargetFollow = GetComponentInChildren<IKTargetFollowVRRig>(true);
        AnimateOnInputScripts = GetComponentsInChildren<AnimateOnInput>(true);
    }

    private void Start()
    {
        // If there's no PhotonView on this object, log a warning and treat as local
        if (photonView == null)
        {
            Debug.LogWarning("NetworkPlayer: No PhotonView found on the player object. Treating as local.", this);
            IsLocalPlayer = true;
        }
        else
        {
            IsLocalPlayer = photonView.IsMine;
        }

        ApplyLocalityRules();
    }

    /// <summary>
    /// Enables or disables local-only components depending on ownership.
    /// Remote instances will have the IK target follow and input-driven animation disabled.
    /// </summary>
    private void ApplyLocalityRules()
    {
        // IK follow
        if (IkTargetFollow != null)
        {
            IkTargetFollow.enabled = IsLocalPlayer;
        }

        // AnimateOnInput scripts
        if (AnimateOnInputScripts != null && AnimateOnInputScripts.Length > 0)
        {
            foreach (var script in AnimateOnInputScripts)
            {
                if (script != null)
                    script.enabled = IsLocalPlayer;
            }
        }

        Debug.Log($"NetworkPlayer: IsLocalPlayer={IsLocalPlayer} on {gameObject.name}");
    }
}
