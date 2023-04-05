package crc648709ebe3dc658b45;


public class Checklist
	extends androidx.fragment.app.Fragment
	implements
		mono.android.IGCUserPeer,
		android.view.View.OnClickListener,
		android.widget.RadioGroup.OnCheckedChangeListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreateView:(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;:GetOnCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_Handler\n" +
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onResume:()V:GetOnResumeHandler\n" +
			"n_onStop:()V:GetOnStopHandler\n" +
			"n_onClick:(Landroid/view/View;)V:GetOnClick_Landroid_view_View_Handler:Android.Views.View/IOnClickListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_onCheckedChanged:(Landroid/widget/RadioGroup;I)V:GetOnCheckedChanged_Landroid_widget_RadioGroup_IHandler:Android.Widget.RadioGroup/IOnCheckedChangeListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("SistemaColeta.Fragments.Checklist, SistemaColeta", Checklist.class, __md_methods);
	}


	public Checklist ()
	{
		super ();
		if (getClass () == Checklist.class)
			mono.android.TypeManager.Activate ("SistemaColeta.Fragments.Checklist, SistemaColeta", "", this, new java.lang.Object[] {  });
	}


	public Checklist (int p0)
	{
		super (p0);
		if (getClass () == Checklist.class)
			mono.android.TypeManager.Activate ("SistemaColeta.Fragments.Checklist, SistemaColeta", "System.Int32, mscorlib", this, new java.lang.Object[] { p0 });
	}


	public android.view.View onCreateView (android.view.LayoutInflater p0, android.view.ViewGroup p1, android.os.Bundle p2)
	{
		return n_onCreateView (p0, p1, p2);
	}

	private native android.view.View n_onCreateView (android.view.LayoutInflater p0, android.view.ViewGroup p1, android.os.Bundle p2);


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


	public void onCheckedChanged (android.widget.RadioGroup p0, int p1)
	{
		n_onCheckedChanged (p0, p1);
	}

	private native void n_onCheckedChanged (android.widget.RadioGroup p0, int p1);

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
