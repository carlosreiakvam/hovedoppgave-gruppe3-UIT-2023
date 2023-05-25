using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Wizard : NetworkBehaviour
{
    private IEnumerator coroutine;
    string[] wizardQuotes =
{
    "The ring, lost to the shadows, must be found!",
    "My magic is weakened without my ring, I need your aid!",
    "The ring, it's more than just a trinket, it's my source of power!",
    "The caves echo with the laughter of those who stole my ring, let's silence them!",
    "With each passing moment, the ring's power could be used for evil. We must hurry!",
    "The ring is not just an object, it's a part of me. I cannot rest until it's found!",
    "The forest whispers of my loss, help me silence its sorrow with the return of my ring.",
    "Your strength and my magic, together we can reclaim what was lost!",
    "Remember, in the cave, danger lurks at every corner. Stay vigilant, my friends!",
    "The ring holds more power than you can imagine!",
    "I sense great danger in the cave...",
    "Take heed, the enemies are stronger than they appear!",
    "There's no time to waste, we must find the ring!",
    "The forest whispers of the ring's presence...",
    "Beware, the path to the ring is treacherous!",
    "The ring is not merely lost, it was taken!",
    "The ring... it yearns to be found.",
    "I can feel the ring's magic, it's close!",
    "The power-ups you find will help us in our quest for the ring."
};

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        ChatManager.Instance.SendMsg("Find the ring and retrieve it to me!", "Wizard");
        StartCoroutine(RepeatMessage());
    }

    private IEnumerator RepeatMessage()
    {
        while (true)
        {
            yield return new WaitForSeconds(90);
            float a = Random.Range(0, 100);
            if (Random.Range(0, 100) > 60)
            {
                ChatManager.Instance.SendMsg(GetRandomQuote(), "Wizard");
            }
        }
    }
    private string GetRandomQuote()
    {
        int randomIndex = Random.Range(0, wizardQuotes.Length);
        return wizardQuotes[randomIndex];
    }




}
