[System.Serializable]
public class AppParam
{
    public int quitAfterSeconds;
    public float lightIntensity;
    public string activeLocation;


    public string toString()
    {
        return "quitAfterSeconds: " + quitAfterSeconds
            + "\n lightIntensity: " + lightIntensity
            + "\n activeLocation: " + activeLocation;
    }
}