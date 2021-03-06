﻿using System;

using OpenTK;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics.Joints;

using Duality.Editor;
using Duality.Resources;

namespace Duality.Components.Physics
{
	/// <summary>
	/// This joint allows the RigidBody to travel on a specific axis. It can be limited to a certain area and driven by a motor force.
	/// </summary>
	[Serializable]
	public sealed class FixedPrismaticJointInfo : JointInfo
	{
		private	Vector2		worldAnchor		= Vector2.Zero;
		private	Vector2		moveAxis		= Vector2.UnitX;
		private	bool		limitEnabled	= false;
		private	float		lowerLimit		= 0.0f;
		private	float		upperLimit		= 0.0f;
		private	bool		motorEnabled	= false;
		private float		maxMotorForce	= 0.0f;
		private float		motorSpeed		= 0.0f;
		private	float		refAngle		= 0.0f;


		public override bool DualJoint
		{
			get { return false; }
		}
		/// <summary>
		/// [GET / SET] The world anchor point to which the RigidBody will be attached.
		/// </summary>
		[EditorHintIncrement(1)]
		public Vector2 WorldAnchor
		{
			get { return this.worldAnchor; }
			set { this.worldAnchor = value; this.UpdateJoint(); }
		}
		/// <summary>
		/// [GET / SET] The axis on which the body may move.
		/// </summary>
		public Vector2 MovementAxis
		{
			get { return this.moveAxis; }
			set { this.moveAxis = value == Vector2.Zero ? Vector2.UnitX : value.Normalized; this.UpdateJoint(); }
		}
		/// <summary>
		/// [GET / SET] Is the joint limited in its movement?
		/// </summary>
		public bool LimitEnabled
		{
			get { return this.limitEnabled; }
			set { this.limitEnabled = value; this.UpdateJoint(); }
		}
		/// <summary>
		/// [GET / SET] The lower joint limit.
		/// </summary>
		[EditorHintIncrement(1)]
		public float LowerLimit
		{
			get { return this.lowerLimit; }
			set { this.lowerLimit = MathF.Min(value, this.upperLimit); this.UpdateJoint(); }
		}
		/// <summary>
		/// [GET / SET] The upper joint limit.
		/// </summary>
		[EditorHintIncrement(1)]
		public float UpperLimit
		{
			get { return this.upperLimit; }
			set { this.upperLimit = MathF.Max(value, this.lowerLimit); this.UpdateJoint(); }
		}
		/// <summary>
		/// [GET / SET] Is the joint motor enabled?
		/// </summary>
		public bool MotorEnabled
		{
			get { return this.motorEnabled; }
			set { this.motorEnabled = value; this.UpdateJoint(); }
		}
		/// <summary>
		/// [GET / SET] The maximum motor force.
		/// </summary>
		public float MaxMotorForce
		{
			get { return this.maxMotorForce; }
			set { this.maxMotorForce = value; this.UpdateJoint(); }
		}
		/// <summary>
		/// [GET / SET] The desired motor speed.
		/// </summary>
		[EditorHintIncrement(1)]
		public float MotorSpeed
		{
			get { return this.motorSpeed; }
			set { this.motorSpeed = value; this.UpdateJoint(); }
		}
		/// <summary>
		/// [GET] The current joint speed.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public float JointSpeed
		{
			get { return this.joint == null ? 0.0f : PhysicsConvert.ToDualityUnit((this.joint as FixedPrismaticJoint).JointSpeed * Time.SPFMult); }
		}
		/// <summary>
		/// [GET] The current joint translation.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public float JointTranslation
		{
			get { return this.joint == null ? 0.0f : PhysicsConvert.ToDualityUnit((this.joint as FixedPrismaticJoint).JointTranslation); }
		}
		/// <summary>
		/// [GET] The current joint motor force.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public float MotorForce
		{
			get { return this.joint == null ? 0.0f : PhysicsConvert.ToDualityUnit((this.joint as FixedPrismaticJoint).MotorForce * Time.SPFMult); }
		}
		/// <summary>
		/// [GET / SET] The reference angle that is used to constrain the bodies angle.
		/// </summary>
		[EditorHintIncrement(MathF.RadAngle1)]
		public float ReferenceAngle
		{
			get { return this.refAngle; }
			set { this.refAngle = MathF.NormalizeAngle(value); this.UpdateJoint(); }
		}


		protected override Joint CreateJoint(Body bodyA, Body bodyB)
		{
			return bodyA != null ? JointFactory.CreateFixedPrismaticJoint(Scene.PhysicsWorld, bodyA, Vector2.Zero, Vector2.UnitX) : null;
		}
		internal override void UpdateJoint()
		{
			base.UpdateJoint();
			if (this.joint == null) return;

			FixedPrismaticJoint j = this.joint as FixedPrismaticJoint;
			j.LocalAnchorA = PhysicsConvert.ToPhysicalUnit(this.worldAnchor);
			j.LocalAnchorB = Vector2.Zero;
			j.ReferenceAngle = this.refAngle;
			j.LocalXAxis1 = this.moveAxis;
			j.LimitEnabled = this.limitEnabled;
			j.LowerLimit = PhysicsConvert.ToPhysicalUnit(this.lowerLimit);
			j.UpperLimit = PhysicsConvert.ToPhysicalUnit(this.upperLimit);
			j.MotorEnabled = this.motorEnabled;
			j.MotorSpeed = PhysicsConvert.ToPhysicalUnit(this.motorSpeed) / Time.SPFMult;
			j.MaxMotorForce = PhysicsConvert.ToPhysicalUnit(this.maxMotorForce) / Time.SPFMult;
		}

		protected override void CopyTo(JointInfo target)
		{
			base.CopyTo(target);
			FixedPrismaticJointInfo c = target as FixedPrismaticJointInfo;
			c.worldAnchor = this.worldAnchor;
			c.moveAxis = this.moveAxis;
			c.refAngle = this.refAngle;
			c.limitEnabled = this.limitEnabled;
			c.lowerLimit = this.lowerLimit;
			c.upperLimit = this.upperLimit;
			c.motorEnabled = this.motorEnabled;
			c.motorSpeed = this.motorSpeed;
			c.maxMotorForce = this.maxMotorForce;
		}
	}
}
