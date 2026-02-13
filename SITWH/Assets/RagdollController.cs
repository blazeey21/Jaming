using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [System.Serializable]
    public struct BoneTargetPair
    {
        public Transform bone;      // Hueso del ragdoll (puede tener o no Rigidbody)
        public Transform target;    // Objeto que se moverá (debe tener WaypointPath)
    }

    public BoneTargetPair[] boneTargets;

    [Header("Movimiento del target entre waypoints")]
    public float targetMoveSpeed = 1f;
    public float arrivalDistance = 0.1f;

    [Header("Seguimiento del hueso hacia el target")]
    public float followSpeed = 2f;
    public bool useSmoothFollow = true;
    public float followSmoothTime = 0.1f;

    [Header("Límites para evitar estiramientos (solo huesos con Rigidbody)")]
    public bool limitDistance = true;
    public float distanceMultiplier = 0.95f;

    // Estado interno
    private class TargetState
    {
        public WaypointPath waypointPath;
        public int currentWaypointIndex = -1;
        public Vector3 targetWaypointPos;
        public Rigidbody boneRigidbody;   // Puede ser null
        public Vector3 boneVelocityRef;    // Para SmoothDamp
        public float boneLength;            // Longitud del hueso (si tiene padre)
        public Transform boneParent;        // Padre del hueso (para límite de distancia)
    }

    private TargetState[] states;

    void Start()
    {
        if (boneTargets == null || boneTargets.Length == 0)
        {
            Debug.LogWarning("No hay bone-targets asignados.");
            return;
        }

        states = new TargetState[boneTargets.Length];

        for (int i = 0; i < boneTargets.Length; i++)
        {
            var pair = boneTargets[i];
            if (pair.bone == null || pair.target == null)
            {
                Debug.LogWarning($"Par {i} tiene bone o target nulo. Se omite.");
                continue;
            }

            var state = new TargetState();

            // Intentar obtener Rigidbody (puede ser null)
            state.boneRigidbody = pair.bone.GetComponent<Rigidbody>();

            // Guardar referencia al padre (para límite de distancia)
            state.boneParent = pair.bone.parent;

            // Calcular longitud del hueso si tiene padre (para límite de distancia)
            if (state.boneParent != null && state.boneRigidbody != null)
            {
                // Buscar un hijo que sea hueso (con Rigidbody) para medir la distancia
                Transform childBone = null;
                foreach (Transform child in pair.bone)
                {
                    if (child.GetComponent<Rigidbody>() != null)
                    {
                        childBone = child;
                        break;
                    }
                }
                if (childBone != null)
                {
                    state.boneLength = Vector3.Distance(pair.bone.position, childBone.position);
                }
                else
                {
                    // Si no hay hijo, estimar con la escala (no ideal, pero funciona)
                    state.boneLength = pair.bone.lossyScale.magnitude * 0.5f;
                    Debug.LogWarning($"No se encontró un hijo para medir la longitud de {pair.bone.name}. Usando valor estimado: {state.boneLength}");
                }
            }

            // Obtener el componente WaypointPath del target
            state.waypointPath = pair.target.GetComponent<WaypointPath>();
            if (state.waypointPath == null)
            {
                Debug.LogWarning($"El target {pair.target.name} no tiene WaypointPath. No se moverá automáticamente.");
            }
            else
            {
                if (state.waypointPath.waypoints == null || state.waypointPath.waypoints.Length == 0)
                {
                    Debug.LogWarning($"El WaypointPath de {pair.target.name} no tiene waypoints. Agrega algunos.");
                }
                else
                {
                    // Elegir un waypoint inicial aleatorio
                    state.targetWaypointPos = state.waypointPath.GetRandomWaypointPosition(-1, out int newIndex);
                    state.currentWaypointIndex = newIndex;
                    Debug.Log($"Target {pair.target.name} empezará en waypoint {newIndex}: {state.targetWaypointPos}");
                }
            }

            states[i] = state;
        }
    }

    void FixedUpdate()
    {
        if (states == null) return;

        for (int i = 0; i < states.Length; i++)
        {
            var state = states[i];
            if (state == null) continue;

            var pair = boneTargets[i];
            if (pair.bone == null || pair.target == null) continue;

            // ----- 1. MOVER EL TARGET HACIA SU WAYPOINT ACTUAL -----
            if (state.waypointPath != null && state.waypointPath.waypoints.Length > 0)
            {
                Vector3 newTargetPos = Vector3.MoveTowards(
                    pair.target.position,
                    state.targetWaypointPos,
                    targetMoveSpeed * Time.fixedDeltaTime
                );
                pair.target.position = newTargetPos;

                if (Vector3.Distance(pair.target.position, state.targetWaypointPos) <= arrivalDistance)
                {
                    state.targetWaypointPos = state.waypointPath.GetRandomWaypointPosition(state.currentWaypointIndex, out int newIndex);
                    state.currentWaypointIndex = newIndex;
                }
            }

            // ----- 2. CALCULAR LA POSICIÓN DESEADA PARA EL HUESO (CON LÍMITE DE DISTANCIA SI TIENE RIGIDBODY) -----
            Vector3 desiredPos = pair.target.position;

            // Solo aplicamos límite si el hueso tiene Rigidbody y padre (para medir distancia)
            if (limitDistance && state.boneRigidbody != null && state.boneParent != null && state.boneLength > 0)
            {
                Vector3 fromParent = desiredPos - state.boneParent.position;
                float distanceFromParent = fromParent.magnitude;
                float maxDistance = state.boneLength * distanceMultiplier;

                if (distanceFromParent > maxDistance)
                {
                    desiredPos = state.boneParent.position + fromParent.normalized * maxDistance;
                }
            }

            // ----- 3. MOVER EL HUESO HACIA LA POSICIÓN DESEADA -----
            if (state.boneRigidbody != null)
            {
                // Hueso con física: usar MovePosition
                if (useSmoothFollow)
                {
                    Vector3 smoothedPos = Vector3.SmoothDamp(
                        pair.bone.position,
                        desiredPos,
                        ref state.boneVelocityRef,
                        followSmoothTime,
                        followSpeed
                    );
                    state.boneRigidbody.MovePosition(smoothedPos);
                }
                else
                {
                    Vector3 newBonePos = Vector3.MoveTowards(pair.bone.position, desiredPos, followSpeed * Time.fixedDeltaTime);
                    state.boneRigidbody.MovePosition(newBonePos);
                }

                // Rotación (opcional)
                state.boneRigidbody.MoveRotation(pair.target.rotation);
            }
            else
            {
                // Hueso sin física: asignación directa de transform
                if (useSmoothFollow)
                {
                    // SmoothDamp no es directamente aplicable a transform, pero podemos usar Lerp
                    pair.bone.position = Vector3.Lerp(pair.bone.position, desiredPos, followSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    pair.bone.position = Vector3.MoveTowards(pair.bone.position, desiredPos, followSpeed * Time.fixedDeltaTime);
                }

                pair.bone.rotation = pair.target.rotation;
            }
        }
    }
}