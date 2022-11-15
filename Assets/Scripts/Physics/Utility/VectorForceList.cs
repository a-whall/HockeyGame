using UnityEngine;
using System.Collections.Generic;

public class VectorForceList {
    List<Vector3> forces;

    public VectorForceList() {
        forces = new List<Vector3>();
    }

    public void Add(Vector3 f) {
        forces.Add(f);
    }

    public Vector3 Total() {
        Vector3 sum = new Vector3();
        forces.ForEach((Vector3 f) => { sum += f; });
        forces.Clear();
        return sum;
    }
}