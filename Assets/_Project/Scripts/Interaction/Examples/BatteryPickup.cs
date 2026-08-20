using UnityEngine;
using SubjectZero.Character.Player;
using SubjectZero.Telemetry;

namespace SubjectZero.Interaction.Examples
{
    public class BatteryPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private float chargeAmountSeconds = 60f;

        public string InteractionPrompt => "Pick Up Battery";

        public bool CanInteract(PlayerController player) => true;

        public void Interact(PlayerController player)
        {
            var flashlight = player.GetComponent<FlashlightController>();
            flashlight?.AddBattery(chargeAmountSeconds);

            TelemetryManager.Instance?.RecordResourceConsumed();
            Destroy(gameObject);
        }
    }
}