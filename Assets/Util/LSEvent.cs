using System.Collections.Generic;

public class LSEvent<T, U>
{
	public delegate void OnInvokeDelegate(T arg0, U arg1);

	public List<OnInvokeDelegate> listeners = new List<OnInvokeDelegate>();

	public void Invoke(T arg0, U arg1)
	{
		foreach (var l in listeners) l.Invoke(arg0, arg1);
	}

	public LSEvent()
	{

	}

	public void AddListener(OnInvokeDelegate d)
	{
		listeners.Add(d);
	}
}

