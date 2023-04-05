package crc648709ebe3dc658b45;


public class Adapter
	extends androidx.fragment.app.FragmentStatePagerAdapter
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_getCount:()I:GetGetCountHandler\n" +
			"n_getItem:(I)Landroidx/fragment/app/Fragment;:GetGetItem_IHandler\n" +
			"n_getPageTitle:(I)Ljava/lang/CharSequence;:GetGetPageTitle_IHandler\n" +
			"";
		mono.android.Runtime.register ("SistemaColeta.Fragments.Adapter, SistemaColeta", Adapter.class, __md_methods);
	}


	public Adapter (androidx.fragment.app.FragmentManager p0)
	{
		super (p0);
		if (getClass () == Adapter.class)
			mono.android.TypeManager.Activate ("SistemaColeta.Fragments.Adapter, SistemaColeta", "AndroidX.Fragment.App.FragmentManager, Xamarin.AndroidX.Fragment", this, new java.lang.Object[] { p0 });
	}


	public Adapter (androidx.fragment.app.FragmentManager p0, int p1)
	{
		super (p0, p1);
		if (getClass () == Adapter.class)
			mono.android.TypeManager.Activate ("SistemaColeta.Fragments.Adapter, SistemaColeta", "AndroidX.Fragment.App.FragmentManager, Xamarin.AndroidX.Fragment:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1 });
	}


	public int getCount ()
	{
		return n_getCount ();
	}

	private native int n_getCount ();


	public androidx.fragment.app.Fragment getItem (int p0)
	{
		return n_getItem (p0);
	}

	private native androidx.fragment.app.Fragment n_getItem (int p0);


	public java.lang.CharSequence getPageTitle (int p0)
	{
		return n_getPageTitle (p0);
	}

	private native java.lang.CharSequence n_getPageTitle (int p0);

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
