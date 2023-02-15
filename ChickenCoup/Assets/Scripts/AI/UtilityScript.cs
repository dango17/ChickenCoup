// Written by Liam Bansal
// Date Created: 6/2/2023

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains a list of motives and actions, and selects an action to execute 
/// based on the highest priority motive.
/// </summary>
public class UtilityScript {
	public struct Motive {
		public delegate float Float();
		public Float insistence;
		public string name;

		public Motive(string name,
			Float insistence) {
			this.name = name;
			this.insistence = insistence;
		}
	}

	public struct Action {
		public bool completed;
		public bool preconditionsMet;
		public delegate bool Bool();
		public KeyValuePair<string, Bool>[] preconditions;
		public KeyValuePair<Motive, float>[] satisfiedMotives;
		public delegate bool ActionToExecute();
		public ActionToExecute action;

		public Action(KeyValuePair<string, Bool>[] preconditions,
			KeyValuePair<Motive, float>[] satisfiedMotives,
			ActionToExecute action) {
			completed = false;
			preconditionsMet = false;
			this.preconditions = preconditions;
			this.satisfiedMotives = satisfiedMotives;
			this.action = action;
		}
	}

	private Action currentAction = default;
	private Motive[] motives = default;
	private Action[] actions = default;

	public UtilityScript(Motive[] motives,
		Action[] actions) {
		this.motives = motives;
		this.actions = actions;
	}

	/// <summary>
	/// Finds and executes the most optimal action for the AI until its completed.
	/// </summary>
	public void Update() {
		if (currentAction.completed || currentAction.action == null) {
			FindExecutableActions();
			FindOptimalAction();
		}

		// A completed action will be the previously executed action.
		if (currentAction.action == null || currentAction.completed) {
			return;
		}

		currentAction.completed = currentAction.action();
	}

	/// <summary>
	/// Sets the current action to its default value.
	/// </summary>
	public void Reset() {
		currentAction = default;
	}

	/// <summary>
	/// Marks any executable action as such, if all of its preconditions are met.
	/// </summary>
	private void FindExecutableActions() {
		for (int i = 0; i < actions.Length; ++i) {
			bool preconditionsMet = true;

			for (int j = 0; j < actions[i].preconditions.Length; ++j) {
				// Check if the precondition is met.
				if (!actions[i].preconditions[j].Value()) {
					preconditionsMet = false;
					break;
				}
			}

			actions[i].preconditionsMet = preconditionsMet;
		}
	}

	/// <summary>
	/// Sets the next action to execute based on which one is executable and
	/// reduces the agent's discontent the most.
	/// </summary>
	private void FindOptimalAction() {
		// Set to a high value to force at least one selection.
		float lowestDiscontent = Mathf.Infinity;

		foreach (Action action in actions) {
			if (action.action == null || !action.preconditionsMet) {
				continue;
			}

			float discontent = 0.0f;

			// Finds the action that reduces the agents discontet the most.
			foreach (KeyValuePair<Motive, float> satisfiedMotive in action.satisfiedMotives) {
				foreach (Motive motive in motives) {
					if (satisfiedMotive.Key.name == motive.name) {
						float loweredInsistence = motive.insistence() - satisfiedMotive.Value;
						// Clamp value to prevent dicontent increasing when
						// insistence is already at 0, because subtracting
						// from it produces a negative number, which turns
						// positive when multiplied by itself.
						loweredInsistence = Mathf.Clamp(loweredInsistence, 0.0f, Mathf.Infinity);
						discontent += loweredInsistence * loweredInsistence;
					} else {
						discontent += motive.insistence() * motive.insistence();
					}
				}
			}

			if (discontent < lowestDiscontent) {
				lowestDiscontent = discontent;
				currentAction = action;
			}
		}
	}
}
