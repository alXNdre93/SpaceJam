using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player player = new Player();

        Enemy enemy1 = new Enemy();
        Enemy enemy2 = new Enemy();

        Weapon weapon1 = new Weapon();
        Weapon weapon2 = new Weapon();
        //Weapon machinewGun = new Weapon("Machine Gun", 5f);

        EnemyType enemyType1 = new EnemyType();
        enemyType1 = EnemyType.MachineGun;

        enemy1.SetEnemyType(enemyType1);

        ///enemy1.weapon = weapon1;
        player.weapon = weapon1;
        enemy2.weapon = weapon2;
    }
}
