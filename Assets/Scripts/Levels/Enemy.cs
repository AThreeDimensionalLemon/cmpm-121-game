using Newtonsoft.Json.Linq;
using UnityEngine;

public class Enemy
{
    public string name;
    public int sprite;
    public int hp;
    public int speed;
    public int damage;

    public Enemy()
    {
        name = "None";
        sprite = -1;
        hp = 0;
        speed = 0;
        damage = 0;
    }

    /* TO READ FROM JSON, USE THIS CODE:
     * 
     *  string EnemiesJsonPath = "enemies";
     *  
     *  JToken json = JToken.Parse(
     *      Resources.Load<TextAsset>(EnemiesJsonPath).text
     *      );
     *  
     *  THEN CONVERT THE PARSED JTokens WITH 
     *  
     *  <token>.ToObject(Enemy)
     *  
     *  FOR STORAGE.
     */

    public override string ToString()
    {
        return name + ": \nindex = " + sprite 
               + ", hp = " + hp + ", speed = " + speed +
               ", damage = " + damage + '\n';
    }
}
