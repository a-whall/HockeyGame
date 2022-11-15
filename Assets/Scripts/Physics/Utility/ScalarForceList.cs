using System.Collections.Generic;

public class ScalarForceList {
    List<float> forces;

    public ScalarForceList() {
        forces = new List<float>();
    }

    public void Add(float f) {
        forces.Add(f);
    }

    public float Total() {
        float sum = 0f;
        forces.ForEach((float f) => { sum += f; });
        forces.Clear();
        return sum;
    }
}