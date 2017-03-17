using System;
using System.Drawing;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Text;
using Wheaty.UnmanagedMetaDataHelper;

namespace TypeRefViewer
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Button buttonBrowse;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonExport;
		#if STUPID_IDE
		private System.Windows.Forms.ImageList imageList1;
		#endif

		// A collection for storing all the namespaces we've
		// seen before, along with their index into the
		// treeview.
		protected ListDictionary imported_namespaces;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			imported_namespaces = new ListDictionary();

			String [] args = Environment.GetCommandLineArgs();
			if ( args.Length > 1 )
				DisplayTypeRefsFromFile( args[1] );				
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		///
#if FOO
		public override void Dispose()
		{
			base.Dispose();
		}
#endif
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form1));
			#if STUPID_IDE
			this.imageList1 = new System.Windows.Forms.ImageList();
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			#endif

			this.treeView1 = new System.Windows.Forms.TreeView();
			this.buttonExport = new System.Windows.Forms.Button();
			this.buttonBrowse = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.treeView1.Location = new System.Drawing.Point(16, 64);
			this.treeView1.Size = new System.Drawing.Size(776, 432);
			this.treeView1.TabIndex = 2;
			this.treeView1.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            this.treeView1.AfterSelect +=
              new System.Windows.Forms.TreeViewEventHandler(treeView1_AfterSelect);
			#if STUPID_IDE
			this.treeView1.SelectedImageIndex = -1;
			this.treeView1.ImageIndex = -1;
			this.treeView1.ImageList = imageList1;
			#endif

			this.buttonExport.Location = new System.Drawing.Point(16, 504);
			this.buttonExport.Size = new System.Drawing.Size(136, 40);
			this.buttonExport.TabIndex = 0;
			this.buttonExport.Text = "Export...";
			this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
			this.buttonExport.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
			this.buttonBrowse.Location = new System.Drawing.Point(16, 8);
			this.buttonBrowse.Size = new System.Drawing.Size(136, 40);
			this.buttonBrowse.TabIndex = 0;
			this.buttonBrowse.Text = "Browse...";
			this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
			this.label1.Location = new System.Drawing.Point(176, 16);
			this.label1.Size = new System.Drawing.Size(608, 32);
			this.label1.TabIndex = 1;
			this.label1.Text = "Click the Browse button to select a .NET file to display";
			this.label2.Location = new System.Drawing.Point(176, 512);
			this.label2.Size = new System.Drawing.Size(608, 32);
			this.label2.TabIndex = 5;
			this.label2.Text = "assembly name";
			this.label2.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(803, 551);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {this.buttonExport,
																		  this.treeView1,
																		  this.label1,
																		  this.label2,
																		  this.buttonBrowse});
			this.Text = "TypeRefViewer - Matt Pietrek 2003";
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		protected bool DisplayTypeRefsFromFile( String filename )
		{
			// Prepare for a fresh start.  Clear out the tree control
			// and any namespaces we saw for the previously viewed
			// assembly
			treeView1.Nodes.Clear();
			imported_namespaces.Clear();

			// Get an instance of our helper class in the C++ DLL
			// This class will call  the unmanaged metadata APIs for us
			TypeRefInfoHelper helper = new TypeRefInfoHelper();

			// Call the helper to return an array of TypeRefInfo's for
			// the given assembly
			TypeRefInfo[] typeRefs = helper.GetTypeRefInfo( filename );

			if ( typeRefs == null )	// Make sure there's something to display!
			{
				MessageBox.Show( "Could not find .NET metadata in this file" );
				return false;
			}

			// Disable treeview redraws while we're stuffing it with data
			treeView1.BeginUpdate();	

			foreach( TypeRefInfo tr in typeRefs )	// For each imported .NET Type...
			{
				try
				{
					String strQualifiedTypeName;

					// If we have the name of the assembly DLL containing the
					// Type, create a partially qualified name.  The
					// Type.GetType() method (below) works better this way!
					if ( tr.m_strAssemblyName.Length > 0 )
					{
						Assembly ass = Assembly.LoadWithPartialName( tr.m_strAssemblyName );
						String x = ass.FullName;
						strQualifiedTypeName = tr.m_strTypeName + "," + x;	// tr.m_strAssemblyName;
					}
					else
						strQualifiedTypeName = tr.m_strTypeName;

					// Get an instance of the Type, given its name
					Type mytype = Type.GetType( strQualifiedTypeName );

					// Get the namespace from the Type instance		
					String strNamespace = mytype.Namespace;

					// Variable for keeping track of where we inserted
					// this TypeRefInfo into the treeview
					int iTreeNodeIndex;

					// Have we already seen other types in this namespace?  If
					// yes, then get the treeview index so that we can
					// insert this type under the namespace node
					if ( imported_namespaces.Contains( strNamespace ) )
					{
						// Fix this cast!
						iTreeNodeIndex = (int)imported_namespaces[ strNamespace ];
					}
					else	// A new namespace we haven't seen before, so
					{		// create a new namespace node for it

						TypeRefTreeNode node = new TypeRefTreeNode( strNamespace, tr.m_strAssemblyName );
						#if STUPID_IDE
						node.SelectedImageIndex = node.ImageIndex = 0;
						#endif
						iTreeNodeIndex = treeView1.Nodes.Add( node );

						// Add knowledge of the new namespace to Dictionary
						imported_namespaces[strNamespace] = iTreeNodeIndex;
					}

					// Figure out the index of the Namespace node that we'll 
					// add the new Type to.
					TypeRefTreeNode namespaceNode = (TypeRefTreeNode)treeView1.Nodes[iTreeNodeIndex];

					// Add the new Type under the namespace node					
					TypeRefTreeNode typeNode = new TypeRefTreeNode( mytype.Name, tr.m_strAssemblyName );
					namespaceNode.Nodes.Add( typeNode );
					#if STUPID_IDE
					typeNode.SelectedImageIndex = typeNode.ImageIndex = 1;
					#endif

					// Iterate through each imported member of the imported
					// Type, and insert it under the Type node we just added.
					foreach ( MemberRefInfo memberRef in tr.m_arMemberRefs )
					{
						// Call reflection method to return all methods
						// with the specified name
						MemberInfo[] members = mytype.GetMember( memberRef.m_strMemberName );

						// If just one member is returned, we know we got the right one
						if ( members.Length == 1 )
						{
							// Call helper function to do pretty things with the
							// member name before addding it.
							AddMemberNodeToTree( members[0], typeNode, memberRef.m_strMemberName );
						}
						else	// This method is overloaded.  See if we can find the right member
						{
							// Get number of params from signature
							int cParams = memberRef.m_signatureBlob[1];

							// If we find a matching method, remember it
							MethodBase matchingMethod = null;

							// Examine each overloaded method to see if its the
							// one we're looking for.  We'll compare each method
							// to the one returned in the MemberRefInfo struct
							foreach ( MethodBase mb in members )
							{
								// Call reflection method to get the parameters
								// for this method
								ParameterInfo [] paramInfo = mb.GetParameters();

								// Does the number of params (as seen via reflection) match
								// the number of params from the signature?  If so, we
								// may have a match.
								if ( paramInfo.Length == cParams )
								{
									// If this is the first match, we *may*
									// have found a match.
									if ( matchingMethod == null )
									{
										matchingMethod = mb;
									}
									else	// A 2nd match.  We didn't find
									{		// it, so bail out
										matchingMethod = null;
										break;
									}
								}
							}

							if ( matchingMethod != null  )
								AddMemberNodeToTree( matchingMethod, typeNode, memberRef.m_strMemberName );
							else
							{
								typeNode.Nodes.Add(
									new TypeRefTreeNode(memberRef.m_strMemberName + " (???) - Overloaded", strNamespace) );
							}
						}
					}
				}
				catch ( Exception e )	// Something crazy went wrong!
				{
					String strException = e.ToString();
					treeView1.Nodes.Add( new TypeRefTreeNode( String.Format("Error with {0}.{1}", tr.m_strAssemblyName, tr.m_strTypeName), "" ) );
				}
			}

			
			treeView1.EndUpdate();	// Let the treeview redraw itself again

			return true;
		}

		static void AddMemberNodeToTree( MemberInfo member, TypeRefTreeNode typeNode, string strMemberName )
		{
			// Given a MemberInfo, decorate it with the parameters and return value
			int memberNode;

			if ( (member.MemberType == MemberTypes.Method) || (member.MemberType == MemberTypes.Constructor) )
			{
				MethodBase method = (MethodBase)member;

				String strParams = FormatParameterString( method.GetParameters() );

				if ( member.MemberType == MemberTypes.Method )	// Normal method
				{
					TypeRefTreeNode n = new TypeRefTreeNode( strMemberName + strParams  + " returns " + ((MethodInfo)method).ReturnType.ToString(), typeNode.m_strAssembly );
					memberNode = typeNode.Nodes.Add( n );
				}
				else											// Constructor
				{
					TypeRefTreeNode n = new TypeRefTreeNode( strMemberName + strParams, typeNode.m_strAssembly );					
					memberNode = typeNode.Nodes.Add( n );
				}
			}
			else
				memberNode = typeNode.Nodes.Add( new TypeRefTreeNode(strMemberName, typeNode.m_strAssembly) );

			#if STUPID_IDE
			memberNode.ImageIndex = -1;
			#endif

		}

		static String FormatParameterString( ParameterInfo[] arParameters )
		{
			StringBuilder str = new StringBuilder();

			str.Append( "(" );

			int paramNumber = 0;

			foreach ( ParameterInfo param in arParameters )
			{
				if ( paramNumber > 0 )	// Tack on a comma before adding the next parameter
					str.Append( ", " );	// But not for the 0'th parameter
				
				str.Append( param.ParameterType );

				String strParamName = param.Name;
				if ( strParamName != null )
				{
					str.Append( " " );
					str.Append( strParamName );
				}

				paramNumber++;
			}

			str.Append( ")" );

			return str.ToString();
		}

		private void buttonBrowse_Click(System.Object sender, System.EventArgs e)
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog();

			openFileDialog1.Filter = "Executable files (*.exe)|*.exe|DLL files (*.dll)|*.dll|All Files (*.*)|*.*"  ;
			openFileDialog1.FilterIndex = 1;
			openFileDialog1.RestoreDirectory = true ;

			if ( openFileDialog1.ShowDialog() == DialogResult.OK )
			{
				bool bSuccess = DisplayTypeRefsFromFile( openFileDialog1.FileName );
				if ( bSuccess )
					label1.Text = openFileDialog1.FileName;
			}
		}
	
		private void buttonExport_Click(System.Object sender, System.EventArgs e)
		{
			// Get the name of the file to write to
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();

			saveFileDialog1.RestoreDirectory = true ;

			if ( saveFileDialog1.ShowDialog() != DialogResult.OK )
				return;

			TextWriter writer = new StreamWriter( saveFileDialog1.FileName );
			TreeNodeCollection assemblyNodes = treeView1.Nodes;

			// Emit some basic header / copyright type info
			writer.Write( "TypeRefViewer - Matt Pietrek, 2001\n\n" );
			writer.Write( "File: {0}\n\n", label1.Text );

			// Iterate through each tree node, and dump it to the file
			// Just minimal formatting is done here
			foreach ( TypeRefTreeNode assemblyNode in assemblyNodes )
			{
				writer.Write( "----------------------------------------\n" );
				writer.Write( "Namespace: {0}\n", assemblyNode.Text );

				foreach ( TypeRefTreeNode classNode in assemblyNode.Nodes )
				{
					writer.Write( "\tclass: {0}\t\tassembly: {1}\n", classNode.Text, classNode.m_strAssembly );

					foreach( TypeRefTreeNode memberNode in classNode.Nodes )
					{
						writer.Write( "\t\t{0}\n", memberNode.Text );
					}
					writer.Write( "\n" );
				}
			}

			writer.Close();
		}

		private void treeView1_AfterSelect( object sender,
                                    TreeViewEventArgs e)
        {
			// Set the form's bottom label to the name of the assembly
			// that we stored in the node.
            label2.Text = ((TypeRefTreeNode)e.Node).m_strAssembly;
        }
	}
}
