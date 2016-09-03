using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Quartz;

namespace QuartzScheduler.Infrastructure.Utils
{
    public static class JobUtil
    {
        public static List<Type> GetJobTypes(string jobsDirectory)
        {
            var jobTypes = new List<Type>();

            var dinfo = new DirectoryInfo(jobsDirectory);
            var files = dinfo.GetFiles("*.dll", SearchOption.AllDirectories).Where(x => x.Name != "Quartz.dll").ToList();

            foreach (var file in files)
            {
                var assembly = Assembly.LoadFile(file.FullName);
                var jobs = assembly.GetTypes()
                    .Where(type => typeof(IJob).IsAssignableFrom(type) && type.IsClass).ToList();

                jobTypes.AddRange(jobs);
            }

            return jobTypes;
        }

        public static Type Create(string assemblyName, string typeName)
        {
            AssemblyName aName = new AssemblyName(assemblyName);
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(
                    aName,
                    AssemblyBuilderAccess.RunAndSave);

            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");

            TypeBuilder tb = mb.DefineType(typeName, TypeAttributes.Public);

            tb.AddInterfaceImplementation(typeof(IJob));

            MethodBuilder meth = tb.DefineMethod(
                "Execute",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(void),
                new[] { typeof(IJobExecutionContext) });

            meth.DefineParameter(1,
                ParameterAttributes.In,
                "context");

            var methIl = meth.GetILGenerator();
            methIl.Emit(OpCodes.Ldarg_0);

            Type t = null;
            try
            {
                // Finish the type.
                t = tb.CreateType();
            }
            catch (Exception ex)
            { }
            
            return t;
        }

        public static bool IsValidCronExpression(string expression)
        {
            return CronExpression.IsValidExpression(expression);
        }
    }
}
