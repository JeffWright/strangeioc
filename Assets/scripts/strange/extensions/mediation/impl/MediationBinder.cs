/**
 * @class strange.extensions.mediation.impl.MediationBinder
 * 
 * Binds Views to Mediators.
 * 
 * Please read strange.extensions.mediation.api.IMediationBinder
 * where I've extensively explained the purpose of View mediation
 */

using System;
using System.Collections;
using UnityEngine;
using strange.extensions.injector.api;
using strange.extensions.mediation.api;
using strange.framework.api;
using strange.framework.impl;

namespace strange.extensions.mediation.impl
{
	public class MediationBinder : Binder, IMediationBinder
	{

		[Inject]
		public IInjectionBinder injectionBinder{ get; set;}

		public MediationBinder ()
		{
		}


		public override IBinding GetRawBinding ()
		{
			return new MediationBinding (resolver) as IBinding;
		}

		public void Trigger(MediationEvent evt, IView view)
		{
			//All views have potential to be injected, regardless of whether they are mediated
			if (evt == MediationEvent.AWAKE)
			{
				initChildren(view);
			}
			Type viewType = view.GetType();
			IMediationBinding binding = GetBinding (viewType) as IMediationBinding;
			if (binding != null)
			{
				switch(evt)
				{
					case MediationEvent.AWAKE:
						mapView (view, binding);
						break;
					case MediationEvent.DESTROYED:
						unmapView (view, binding);
						break;
					default:
						break;
				}
			}
		}
		
		/// Initialize all IViews within this view
		virtual protected void initChildren(IView view)
		{
			MonoBehaviour mono = view as MonoBehaviour;
			Component[] views = mono.GetComponentsInChildren(typeof(IView), true) as Component[];
			
			int aa = views.Length;
			for (int a = 0; a < aa; a++)
			{
				(view as IView).registeredWithContext = true;
				injectionBinder.injector.Inject (views[a], false);
			}
		}

		public override IBinding Bind<T> ()
		{
			injectionBinder.Bind<T> ().To<T>();
			return base.Bind<T> ();
		}

		/// Creates and registers a Mediator for a specific View instance.
		/// Takes a specific View instance and a binding and, if a binding is found for that type, creates and registers a Mediator.
		virtual protected void mapView(IView view, IMediationBinding binding)
		{
			Type viewType = view.GetType();

			if (bindings.ContainsKey(viewType))
			{
				object[] values = binding.value as object[];
				int aa = values.Length;
				for (int a = 0; a < aa; a++)
				{
					MonoBehaviour mono = view as MonoBehaviour;
					Type mediatorType = values [a] as Type;
					IMediator mediator = mono.gameObject.AddComponent(mediatorType) as IMediator;
					mediator.setViewComponent (mono);
					mediator.preRegister ();
					injectionBinder.injector.Inject (mediator);
					mediator.onRegister ();
				}
			}
		}

		/// Removes a mediator when its view is destroyed
		virtual protected void unmapView(IView view, IMediationBinding binding)
		{
			Type viewType = view.GetType();

			if (bindings.ContainsKey(viewType))
			{
				object[] values = binding.value as object[];
				int aa = values.Length;
				for (int a = 0; a < aa; a++)
				{
					Type mediatorType = values[a] as Type;
					MonoBehaviour mono = view as MonoBehaviour;
					IMediator mediator = mono.GetComponent(mediatorType) as IMediator;
					if (mediator != null)
					{
						mediator.onRemove();
					}
				}
			}
		}

		private void enableView(IView view)
		{
		}

		private void disableView(IView view)
		{
		}
	}
}
