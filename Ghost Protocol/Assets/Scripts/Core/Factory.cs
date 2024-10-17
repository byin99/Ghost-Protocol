using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : Singleton<Factory>
{
    ShellPool shell;

    protected override void OnInitialize()
    {
        shell = GetComponentInChildren<ShellPool>();
        if (shell != null)
            shell.Initialize();
    }

    public Shell GetShell(Vector3? position = null, Vector3? eulerAngle = null)
    {
        return shell.GetObject(position, eulerAngle);
    }
}
