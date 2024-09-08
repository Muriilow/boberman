using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
public interface IPickable
{
    CountdownTimer Timer { get; set; }
    void DesappearServerRpc();
}
