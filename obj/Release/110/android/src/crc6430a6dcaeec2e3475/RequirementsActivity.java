package crc6430a6dcaeec2e3475;


public class RequirementsActivity
	extends crc6430a6dcaeec2e3475.BaseActivity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onResume:()V:GetOnResumeHandler\n" +
			"";
		mono.android.Runtime.register ("SistemaColeta.Activities.RequirementsActivity, SistemaColeta", RequirementsActivity.class, __md_methods);
	}


	public RequirementsActivity ()
	{
		super ();
		if (getClass () == RequirementsActivity.class)
			mono.android.TypeManager.Activate ("SistemaColeta.Activities.RequirementsActivity, SistemaColeta", "", this, new java.lang.Object[] {  });
	}


	public RequirementsActivity (int p0)
	{
		super (p0);
		if (getClass () == RequirementsActivity.class)
			mono.android.TypeManager.Activate ("SistemaColeta.Activities.RequirementsActivity, SistemaColeta", "System.Int32, mscorlib", this, new java.lang.Object[] { p0 });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onResume ()
	{
		n_onResume ();
	}

	private native void n_onResume ();

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
