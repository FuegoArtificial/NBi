﻿using System;
using System.Collections.Generic;
using NBi.NUnit.Builder;
using NBi.Xml.Constraints;
using NBi.Xml.Systems;
using System.Linq;
using NUnitCtr = NUnit.Framework.Constraints;
using NBi.Framework;

namespace NBi.NUnit
{
	public class TestCaseFactory
	{
        private readonly ICollection<BuilderRegistration> registrations;
        private readonly ITestConfiguration configuration;

        public TestCaseFactory()
            : this(TestConfiguration.Default)
        {
        }

        public TestCaseFactory(ITestConfiguration configuration)
		{
            this.configuration = configuration;
			registrations = new List<BuilderRegistration>();
			RegisterDefault();
		}

		/// <summary>
		/// Register the values for production usage
		/// </summary>
		private void RegisterDefault()
		{
			Register (typeof(ExecutionXml), typeof(FasterThanXml), new ExecutionFasterThanChooser());
			Register (typeof(ExecutionXml), typeof(SyntacticallyCorrectXml),new ExecutionSyntacticallyCorrectBuilder());
			Register (typeof(ExecutionXml), typeof(EqualToXml),new ExecutionEqualToBuilder());
			Register(typeof(ExecutionXml), typeof(MatchPatternXml), new ExecutionMatchPatternBuilder());
            Register(typeof(ExecutionXml), typeof(EvaluateRowsXml), new ExecutionEvaluateRowsBuilder());
            Register(typeof(ExecutionXml), typeof(SuccessfulXml), new ExecutionNonQuerySuccessfulBuilder());
            Register(typeof(ExecutionXml), typeof(RowCountXml), new ExecutionRowCountBuilder());
			
			Register (typeof(MembersXml), typeof(CountXml) ,new MembersCountBuilder());
			Register(typeof(MembersXml), typeof(OrderedXml), new MembersOrderedBuilder());
			Register(typeof(MembersXml), typeof(ContainXml), new MembersContainBuilder());
			Register(typeof(MembersXml), typeof(EquivalentToXml), new MembersEquivalentToBuilder());
			Register(typeof(MembersXml), typeof(SubsetOfXml), new MembersSubsetOfBuilder());
			Register(typeof(MembersXml), typeof(MatchPatternXml), new MembersMatchPatternBuilder());

			Register (typeof(StructureXml), typeof(ContainXml),new StructureContainBuilder());
			Register(typeof(StructureXml), typeof(EquivalentToXml), new StructureEquivalentToBuilder());
			Register(typeof(StructureXml), typeof(SubsetOfXml), new StructureSubsetOfBuilder());
			Register(typeof(StructureXml), typeof(ExistsXml), new StructureExistsBuilder());
			Register(typeof(StructureXml), typeof(LinkedToXml), new StructureLinkedToBuilder());
		}

		/// <summary>
		/// Register a new builder for corresponding types. If a builder was already existing for this association, it's replaced by the new one
		/// </summary>
		/// <param name="sutType">Type of System Under Test</param>
		/// <param name="ctrType">Type of Constraint</param>
		/// <param name="builder">Instance of builder deicated for these types of System Under Test and Constraint</param>
		internal void Register(Type sutType, Type ctrType, ITestCaseBuilder builder)
		{
			if (IsHandling(sutType, ctrType))
				registrations.FirstOrDefault(reg => reg.SystemUnderTestType.Equals(sutType) && reg.ConstraintType.Equals(ctrType)).Builder = builder;
			else
				registrations.Add(new BuilderRegistration(sutType, ctrType, builder));
		}

        internal void Register(Type sutType, Type ctrType, ITestCaseBuilderChooser chooser)
        {
            if (IsHandling(sutType, ctrType))
                registrations.FirstOrDefault(reg => reg.SystemUnderTestType.Equals(sutType) && reg.ConstraintType.Equals(ctrType)).Chooser = chooser;
            else
                registrations.Add(new BuilderRegistration(sutType, ctrType, chooser));
        }

		internal bool IsHandling(Type sutType, Type ctrType)
		{
			var existing = registrations.FirstOrDefault(reg => reg.SystemUnderTestType.Equals(sutType) && reg.ConstraintType.Equals(ctrType));
			return (existing != null);
		}

		internal class BuilderRegistration
		{
			public Type SystemUnderTestType {get; set;}
			public Type ConstraintType {get; set;}
			public ITestCaseBuilder Builder {get; set;}
            public ITestCaseBuilderChooser Chooser { get; set; }

            private BuilderRegistration(Type sutType, Type ctrType)
            {
                SystemUnderTestType = sutType;
                ConstraintType = ctrType;
            }

			public BuilderRegistration (Type sutType, Type ctrType, ITestCaseBuilder builder)
                : this(sutType, ctrType)
			{
				Builder = builder;
			}

            public BuilderRegistration (Type sutType, Type ctrType, ITestCaseBuilderChooser chooser)
                : this (sutType, ctrType)
            {
                Chooser = chooser;
                Chooser.Target = this;
            }
		}

		/// <summary>
		/// Create a new instance of a test case
		/// </summary>
		/// <param name="sutXml"></param>
		/// <param name="ctrXml"></param>
		/// <returns></returns>
		public TestCase Instantiate(AbstractSystemUnderTestXml sutXml, AbstractConstraintXml ctrXml)
		{
			if (sutXml == null)
				throw new ArgumentNullException("sutXml");
			if (ctrXml == null)
				throw new ArgumentNullException("ctrXml");

			ITestCaseBuilder builder = null;

			//Look for registration ...
			var registration = registrations.FirstOrDefault(reg => sutXml.GetType()==reg.SystemUnderTestType && ctrXml.GetType() == reg.ConstraintType);
			if (registration == null)
				throw new ArgumentException(string.Format("'{0}' is not an expected type for a constraint with a system-under-test '{1}'.", ctrXml.GetType().Name, sutXml.GetType().Name));

            //Apply the chooser if needed
            if (registration.Builder == null)
                registration.Chooser.Choose(sutXml, ctrXml);

			//Get Builder and initiate it
			builder = registration.Builder;
			builder.Setup(sutXml, ctrXml, configuration);
			
			//Build
			builder.Build();
            NUnitCtr.Constraint ctr = builder.GetConstraint();
			var sut = builder.GetSystemUnderTest();

			//Apply negation if needed
			if (ctrXml.Not)
                ctr = new NUnitCtr.NotConstraint(ctr);

			return new TestCase(sut, ctr);
		}
	}
}
