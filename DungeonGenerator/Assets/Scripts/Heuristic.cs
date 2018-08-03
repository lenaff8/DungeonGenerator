using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heuristic : MonoBehaviour {

    // STRUCTURES
    public float spikes, fallingRocks, projectiles, laser;
    
    // TOOLS
    public float claw, pickaxe, jetpack, bombs, crampons; //perforadora
    private float totalUses;

    // HITS COUNT
    public float spikesCount, fallingRocksCount, projectilesCount, laserCount, waterCount, lavaCount;
    private float totalHits;

    // TOOLS USED COUNT
    public float clawCount, pickaxeCount, jetpackCount, bombsCount, cramponsCount; //perforadora

    // ENVIRONMENT
    public float temperature, humidity, gas, rockHardness, rockDispersion;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void RandomValues(int seed)
    {
        Random.InitState(seed);

        spikes = Random.value;
        fallingRocks = Random.value;
        projectiles = Random.value;
        laser = Random.value;

        claw = Random.value;
        pickaxe = Random.value;
        jetpack = Random.value;
        bombs = Random.value;
        crampons = Random.value;

        rockHardness = Random.value;
        rockDispersion = Random.value;
        gas = Random.value;
        humidity = Random.value;
        temperature = Random.value;

    }

    void CalculateValues()
    {
        totalHits = spikesCount + fallingRocksCount + projectilesCount + laserCount + waterCount + lavaCount;
        totalUses = clawCount + pickaxeCount + jetpackCount + bombsCount + cramponsCount;

        spikes = spikesCount / totalHits;
        fallingRocks = fallingRocksCount / totalHits;
        projectiles = projectilesCount / totalHits;
        laser = laserCount / totalHits;

        claw = clawCount / totalUses;
        pickaxe = pickaxeCount / totalUses;
        jetpack = jetpackCount / totalUses;
        bombs = bombsCount / totalUses;
        crampons = cramponsCount / totalUses;

        rockHardness = pickaxeCount / (pickaxeCount + cramponsCount + clawCount);
        gas = jetpackCount / totalUses;
        rockDispersion = bombsCount / (bombsCount + pickaxeCount);
        humidity = waterCount / totalHits;
        temperature = lavaCount / totalHits;
        //temperature = ((lavaCount / totalHits) + (jetpackCount / totalUses));
    }
}
