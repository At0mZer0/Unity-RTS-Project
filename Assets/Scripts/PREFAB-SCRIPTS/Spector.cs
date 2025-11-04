using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Spector : MonoBehaviour {
    
    public AttackController atkCtrl;
    public Unit unit;
    public ResourceGatherer gatherer;
    public Animator anim;
    public NavMeshAgent navAgent;
    public Renderer[] renderers;

    public bool isSpector = true;
    public float speedMod = 1.25f;
    public float pulseMin = 0.5f;
    public float pulseMax = 1.5f;
    public float pulseSpeed = 2.0f;
    public Color[] originalColors;
    public Color spectorColor; 


    void Start() {
        // get references for Spector special behaviors
        atkCtrl = GetComponent<AttackController>();
        unit = GetComponent<Unit>();
        gatherer = GetComponent<ResourceGatherer>();
        anim = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();

        // get all renderers for controlling material effects
        renderers = GetComponentsInChildren<Renderer>(); 
        // store original colors to reset 
        originalColors = new Color[renderers.Length];
        spectorColor = new Color(0.2f, 0.7f, 0.9f); // light blue color for spector


         // Disable resource gathering
        if (gatherer != null) {
            gatherer.enabled = false;

            // reset any possible resource gahtering properties
            gatherer.targetResourceNode = null;
            gatherer.currentResourceType = ResourceManager.ResourceType.None;
            gatherer.currentLoad = 0;
            gatherer.hasRune = false;

            // stop animator from state changes
            if (anim != null) {
                // need to pass an int into anim properties so cast Enum to Int and pass in the prop name as string
                anim.SetInteger("ResourceState", (int)ResourceState.Idle);  // possibly add a bool isSpector to anim to prevent state changes that way if this doesn't work well
            }

            // change speed for spectors
            if (navAgent != null) {
                navAgent.speed *= speedMod; 
            }
        }
        StartCoroutine(SpectorEffects());
    }

    private IEnumerator SpectorEffects() {
        
        for (int i = 0; i < renderers.Length; i++) {
            Material[] materials = renderers[i].materials;

            foreach (Material mat in materials) {
                //enable emission
                mat.EnableKeyword("_EMISSION");
                originalColors[i] = mat.GetColor("_EmissionColor");
            }
        }
        while (true) {
            // add 1 to bring the Sin value to 0-2 and multiply by .5 to bring it to 0-1 range
            float emission = Mathf.Lerp(pulseMin, pulseMax,(Mathf.Sin(Time.time * pulseSpeed) + 1.0f) * 0.5f); 
            
            // Apply to all materials
            foreach (Renderer renderer in renderers) {
                foreach (Material mat in renderer.materials) {
                    mat.SetColor("_EmissionColor", spectorColor * emission);
                }
            }
            yield return null;
        }
    }
}
