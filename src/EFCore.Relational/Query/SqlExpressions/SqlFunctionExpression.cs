// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class SqlFunctionExpression : SqlExpression
    {
        public SqlFunctionExpression(
            [NotNull] string functionName,
            bool nullable,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(instance: null, schema: null, functionName, nullable, instancePropagatesNullability: null, builtIn: true, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            [NotNull] string schema,
            [NotNull] string functionName,
            bool nullable,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(instance: null, Check.NotEmpty(schema, nameof(schema)), functionName, nullable, instancePropagatesNullability: null, builtIn: false, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            [NotNull] SqlExpression instance,
            [NotNull] string functionName,
            bool nullable,
            bool instancePropagatesNullability,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(Check.NotNull(instance, nameof(instance)), schema: null, functionName, nullable, instancePropagatesNullability, builtIn: true, type, typeMapping)
        {
        }

        private SqlFunctionExpression(
            [CanBeNull] SqlExpression instance,
            [CanBeNull] string schema,
            [NotNull] string name,
            bool nullable,
            bool? instancePropagatesNullability,
            bool builtIn,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(instance, schema, name, niladic: true, arguments: null, nullable, instancePropagatesNullability, argumentsPropagateNullability: null, builtIn, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            [NotNull] string functionName,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(instance: null, schema: null, functionName, arguments, nullable, instancePropagatesNullability: null, argumentsPropagateNullability,  builtIn: true, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            [CanBeNull] string schema,
            [NotNull] string functionName,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(instance: null, Check.NullButNotEmpty(schema, nameof(schema)), functionName, arguments, nullable, instancePropagatesNullability: null, argumentsPropagateNullability, builtIn: false, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            [NotNull] SqlExpression instance,
            [NotNull] string functionName,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            bool instancePropagatesNullability,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(Check.NotNull(instance, nameof(instance)), schema: null, functionName, arguments, nullable, instancePropagatesNullability, argumentsPropagateNullability, builtIn: true, type, typeMapping)
        {
        }

        private SqlFunctionExpression(
            [CanBeNull] SqlExpression instance,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            bool? instancePropagatesNullability,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            bool builtIn,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(instance, schema, name, niladic: false, Check.NotNull(arguments, nameof(arguments)), nullable, instancePropagatesNullability, Check.NotNull(argumentsPropagateNullability, nameof(argumentsPropagateNullability)) , builtIn, type, typeMapping)
        {
        }

        private SqlFunctionExpression(
            [CanBeNull] SqlExpression instance,
            [CanBeNull] string schema,
            [NotNull] string name,
            bool niladic,
            [CanBeNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            bool? instancePropagatesNullability,
            [CanBeNull] IEnumerable<bool> argumentsPropagateNullability,
            bool builtIn,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));

            Instance = instance;
            Name = name;
            Schema = schema;
            IsNiladic = niladic;
            IsBuiltIn = builtIn;
            Arguments = arguments?.ToList();
            IsNullable = nullable;
            InstancePropagatesNullability = instancePropagatesNullability;
            ArgumentsPropagateNullability = argumentsPropagateNullability?.ToList();
        }

        public virtual string Name { get; }
        public virtual string Schema { get; }
        public virtual bool IsNiladic { get; }
        public virtual bool IsBuiltIn { get; }
        public virtual IReadOnlyList<SqlExpression> Arguments { get; }
        public virtual SqlExpression Instance { get; }

        public virtual bool IsNullable { get; private set; }

        public virtual bool? InstancePropagatesNullability { get; private set; }

        public virtual IReadOnlyList<bool> ArgumentsPropagateNullability { get; private set; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var changed = false;
            var instance = (SqlExpression)visitor.Visit(Instance);
            changed |= instance != Instance;

            SqlExpression[] arguments = default;
            if (!IsNiladic)
            {
                arguments = new SqlExpression[Arguments.Count];
                for (var i = 0; i < arguments.Length; i++)
                {
                    arguments[i] = (SqlExpression)visitor.Visit(Arguments[i]);
                    changed |= arguments[i] != Arguments[i];
                }
            }

            return changed
                ? new SqlFunctionExpression(
                    instance,
                    Schema,
                    Name,
                    IsNiladic,
                    arguments,
                    IsNullable,
                    InstancePropagatesNullability,
                    ArgumentsPropagateNullability,
                    IsBuiltIn,
                    Type,
                    TypeMapping)
                : this;
        }

        public virtual SqlFunctionExpression ApplyTypeMapping([CanBeNull] RelationalTypeMapping typeMapping)
            => new SqlFunctionExpression(
                Instance,
                Schema,
                Name,
                IsNiladic,
                Arguments,
                IsNullable,
                InstancePropagatesNullability,
                ArgumentsPropagateNullability,
                IsBuiltIn,
                Type,
                typeMapping ?? TypeMapping);

        public virtual SqlFunctionExpression Update([CanBeNull] SqlExpression instance, [CanBeNull] IReadOnlyList<SqlExpression> arguments)
        {
            return instance != Instance || !arguments?.SequenceEqual(Arguments) == true
                ? new SqlFunctionExpression(
                    instance,
                    Schema,
                    Name,
                    IsNiladic,
                    arguments,
                    IsNullable,
                    InstancePropagatesNullability,
                    ArgumentsPropagateNullability,
                    IsBuiltIn,
                    Type,
                    TypeMapping)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.Append(Schema).Append(".").Append(Name);
            }
            else
            {
                if (Instance != null)
                {
                    expressionPrinter.Visit(Instance);
                    expressionPrinter.Append(".");
                }

                expressionPrinter.Append(Name);
            }

            if (!IsNiladic)
            {
                expressionPrinter.Append("(");
                expressionPrinter.VisitCollection(Arguments);
                expressionPrinter.Append(")");
            }
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SqlFunctionExpression sqlFunctionExpression
                    && Equals(sqlFunctionExpression));

        private bool Equals(SqlFunctionExpression sqlFunctionExpression)
            => base.Equals(sqlFunctionExpression)
                && string.Equals(Name, sqlFunctionExpression.Name)
                && string.Equals(Schema, sqlFunctionExpression.Schema)
                && ((Instance == null && sqlFunctionExpression.Instance == null)
                    || (Instance != null && Instance.Equals(sqlFunctionExpression.Instance)))
                && Arguments.SequenceEqual(sqlFunctionExpression.Arguments);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Name);
            hash.Add(IsNiladic);
            hash.Add(Schema);
            hash.Add(Instance);
            for (var i = 0; i < Arguments.Count; i++)
            {
                hash.Add(Arguments[i]);
            }

            return hash.ToHashCode();
        }
    }
}
