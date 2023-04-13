using UnityEngine;

[CreateAssetMenu]
public class PlayerNameSO : ScriptableObject
{
#if UNITY_EDITOR
    [Multiline]
    public string DeveloperDescription = "";
#endif

    public string Value;

    public void SetValue(string value)
    {
        Value = value;
    }

    public void SetValue(PlayerNameSO value)
    {
        Value = value.Value;
    }

}