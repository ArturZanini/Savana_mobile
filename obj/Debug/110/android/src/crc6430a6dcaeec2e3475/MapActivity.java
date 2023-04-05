package crc6430a6dcaeec2e3475;


public class MapActivity
	extends crc6430a6dcaeec2e3475.BaseActivity
	implements
		mono.android.IGCUserPeer,
		android.view.View.OnClickListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onStop:()V:GetOnStopHandler\n" +
			"n_onClick:(Landroid/view/View;)V:GetOnClick_Landroid_view_View_Handler:Android.Views.View/IOnClickListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("SistemaColeta.Activities.MapActivity, SistemaColeta", MapActivity.class, __md_methods);
	}


	public MapActivity ()
	{
		super ();
		if (getClass () == MapActivity.class)
			mono.android.TypeManager.Activate ("SistemaColeta.Activities.MapActivity, SistemaColeta", "", this, new java.lang.Object[] {  });
	}


	public MapActivity (int p0)
	{
		super (p0);
		if (getClass () == MapActivity.class)
			mono.android.TypeManager.Activate ("SistemaColeta.Activities.MapActivity, SistemaColeta", "System.Int32, mscorlib", this, new java.lang.Object[] { p0 });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onStop ()
	{
		n_onStop ();
	}

	private native void n_onStop ();


	public void onClick (android.view.View p0)
	{
		n_onClick (p0);
	}

	private native void n_onClick (android.view.View p0);

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
