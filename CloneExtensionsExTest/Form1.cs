using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloneExtensionsExTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dynamic ssss1 = new System.Dynamic.ExpandoObject();
            ssss1.aaaaa = new { };
            dynamic ssss2 = new System.Dynamic.ExpandoObject();
            ssss2.aaaaa = "dddddddd";
            ssss2.bbbbb = 5;

            CloneExtensionsEx.CloneFactory.CustomInitializers.Add(typeof(IDictionary<string, object>), f => new Dictionary<string, object>());
            //var s1 = CloneExtensionsEx.CloneFactory.GetClone(ssss1 as IDictionary<string, object>, CloneExtensionsEx.CloneFactory.CustomInitializers, customResolveFun:f=> 
            //{

            //});
            var ssssss = CloneExtensionsEx.CloneFactory.GetClone("ssssss");
            var s2 = CloneExtensionsEx.CloneFactory.GetClone((System.Dynamic.ExpandoObject)ssss2,
                (f, d) =>
            {
                if (f == typeof(System.Dynamic.ExpandoObject))
                {
                    return new System.Dynamic.ExpandoObject();
                }
                else if (f == typeof(object) && (d.GetType() == typeof(string) || d.GetType().IsValueType))
                {
                    return d;
                }
                else if (f == typeof(object) && d.GetType() != typeof(object))
                {
                    return Activator.CreateInstance(d.GetType());
                }
                return new object();
            },
            f =>
            {

            });
        }
    }
}