/*
   Copyright 2012-2016 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using RDFSharp.Model;

namespace RDFSharp.Store
{

    /// <summary>
    /// RDFSQLiteStore represents a store backed on SQLite engine
    /// </summary>
    public class RDFSQLiteStore: RDFStore {

        #region Properties
        /// <summary>
        /// Connection to the SQLite database
        /// </summary>
        public SQLiteConnection Connection { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a SQLite store instance at the given path
        /// </summary>
        public RDFSQLiteStore(String sqliteDbPath) {
            if(sqliteDbPath    != null) {

                //Initialize store structures
                this.StoreType  = "SQLITE";
                this.Connection = new SQLiteConnection(@"Data Source=" + sqliteDbPath +
                                                        ";Version=3;FailIfMissing=False;");
                this.StoreID    = RDFModelUtilities.CreateHash(this.ToString());

                //Clone internal store template
                if(!File.Exists(sqliteDbPath)) {
                    try {
                        Assembly sqlite         = Assembly.GetExecutingAssembly();
                        using(var templateDB    = sqlite.GetManifestResourceStream("RDFSharp.Store.Template.RDFSQLiteTemplate.db")) {
                            using(var destineDB = new FileStream(sqliteDbPath, FileMode.Create, FileAccess.ReadWrite)) {
                                templateDB.CopyTo(destineDB);
                            }
                        }
                    }
                    catch(Exception ex) {
                        throw new RDFStoreException("Cannot create SQLite store because: " + ex.Message, ex);
                    }
                }

                //Perform initial diagnostics
                else {
                    this.PrepareStore();
                }

            }
            else {
                throw new RDFStoreException("Cannot connect to SQLite store because: given \"sqliteDbPath\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SQLite store 
        /// </summary>
        public override String ToString() {
            return base.ToString() + "|CONNECTION=" + this.Connection.ConnectionString;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Merges the given graph into the store, avoiding duplicate insertions
        /// </summary>
        public override RDFStore MergeGraph(RDFGraph graph) {
            if (graph       != null) {
                var graphCtx = new RDFContext(graph.Context);

                //Create command
                var command  = new SQLiteCommand("INSERT OR IGNORE INTO main.Quadruples(QuadrupleID, TripleFlavor, Context, ContextID, Subject, SubjectID, Predicate, PredicateID, Object, ObjectID) VALUES (@QID, @TFV, @CTX, @CTXID, @SUBJ, @SUBJID, @PRED, @PREDID, @OBJ, @OBJID)", this.Connection);
                command.Parameters.Add(new SQLiteParameter("QID",    DbType.Int64));
                command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                command.Parameters.Add(new SQLiteParameter("CTX",    DbType.String, 1000));
                command.Parameters.Add(new SQLiteParameter("CTXID",  DbType.Int64));
                command.Parameters.Add(new SQLiteParameter("SUBJ",   DbType.String, 1000));
                command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                command.Parameters.Add(new SQLiteParameter("PRED",   DbType.String, 1000));
                command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                command.Parameters.Add(new SQLiteParameter("OBJ",    DbType.String, 5000));
                command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));

                try {

                    //Open connection
                    this.Connection.Open();

                    //Prepare command
                    command.Prepare();

                    //Open transaction
                    command.Transaction = this.Connection.BeginTransaction();

                    //Iterate triples
                    foreach(var triple in graph) {

                        //Valorize parameters
                        command.Parameters["QID"].Value    = RDFModelUtilities.CreateHash(graphCtx         + " " +
                                                                                          triple.Subject   + " " +
                                                                                          triple.Predicate + " " +
                                                                                          triple.Object);
                        command.Parameters["TFV"].Value    = triple.TripleFlavor;
                        command.Parameters["CTX"].Value    = graphCtx.ToString();
                        command.Parameters["CTXID"].Value  = graphCtx.PatternMemberID;
                        command.Parameters["SUBJ"].Value   = triple.Subject.ToString();
                        command.Parameters["SUBJID"].Value = triple.Subject.PatternMemberID;
                        command.Parameters["PRED"].Value   = triple.Predicate.ToString();
                        command.Parameters["PREDID"].Value = triple.Predicate.PatternMemberID;
                        command.Parameters["OBJ"].Value    = triple.Object.ToString();
                        command.Parameters["OBJID"].Value  = triple.Object.PatternMemberID;

                        //Execute command
                        command.ExecuteNonQuery();
                    }

                    //Close transaction
                    command.Transaction.Commit();

                    //Close connection
                    this.Connection.Close();

                }
                catch (Exception ex) {

                    //Rollback transaction
                    command.Transaction.Rollback();

                    //Close connection
                    this.Connection.Close();

                    //Propagate exception
                    throw new RDFStoreException("Cannot insert data into SQLite store because: " + ex.Message, ex);

                }

            }
            return this;
        }

        /// <summary>
        /// Adds the given quadruple to the store, avoiding duplicate insertions
        /// </summary>
        public override RDFStore AddQuadruple(RDFQuadruple quadruple) {
            if (quadruple   != null) {

                //Create command
                var command  = new SQLiteCommand("INSERT OR IGNORE INTO main.Quadruples(QuadrupleID, TripleFlavor, Context, ContextID, Subject, SubjectID, Predicate, PredicateID, Object, ObjectID) VALUES (@QID, @TFV, @CTX, @CTXID, @SUBJ, @SUBJID, @PRED, @PREDID, @OBJ, @OBJID)", this.Connection);
                command.Parameters.Add(new SQLiteParameter("QID",    DbType.Int64));
                command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                command.Parameters.Add(new SQLiteParameter("CTX",    DbType.String, 1000));
                command.Parameters.Add(new SQLiteParameter("CTXID",  DbType.Int64));
                command.Parameters.Add(new SQLiteParameter("SUBJ",   DbType.String, 1000));
                command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                command.Parameters.Add(new SQLiteParameter("PRED",   DbType.String, 1000));
                command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                command.Parameters.Add(new SQLiteParameter("OBJ",    DbType.String, 5000));
                command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));

                //Valorize parameters
                command.Parameters["QID"].Value    = quadruple.QuadrupleID;
                command.Parameters["TFV"].Value    = quadruple.TripleFlavor;
                command.Parameters["CTX"].Value    = quadruple.Context.ToString();
                command.Parameters["CTXID"].Value  = quadruple.Context.PatternMemberID;
                command.Parameters["SUBJ"].Value   = quadruple.Subject.ToString();
                command.Parameters["SUBJID"].Value = quadruple.Subject.PatternMemberID;
                command.Parameters["PRED"].Value   = quadruple.Predicate.ToString();
                command.Parameters["PREDID"].Value = quadruple.Predicate.PatternMemberID;
                command.Parameters["OBJ"].Value    = quadruple.Object.ToString();
                command.Parameters["OBJID"].Value  = quadruple.Object.PatternMemberID;

                try {

                    //Open connection
                    this.Connection.Open();

                    //Prepare command
                    command.Prepare();

                    //Open transaction
                    command.Transaction = this.Connection.BeginTransaction();

                    //Execute command
                    command.ExecuteNonQuery();

                    //Close transaction
                    command.Transaction.Commit();

                    //Close connection
                    this.Connection.Close();

                }
                catch (Exception ex) {

                    //Rollback transaction
                    command.Transaction.Rollback();

                    //Close connection
                    this.Connection.Close();

                    //Propagate exception
                    throw new RDFStoreException("Cannot insert data into SQLite store because: " + ex.Message, ex);

                }

            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given quadruples from the store
        /// </summary>
        public override RDFStore RemoveQuadruple(RDFQuadruple quadruple) {
            if (quadruple  != null) {

                //Create command
                var command = new SQLiteCommand("DELETE FROM main.Quadruples WHERE QuadrupleID = @QID", this.Connection);
                command.Parameters.Add(new SQLiteParameter("QID", DbType.Int64));

                //Valorize parameters
                command.Parameters["QID"].Value = quadruple.QuadrupleID;

                try {

                    //Open connection
                    this.Connection.Open();

                    //Prepare command
                    command.Prepare();

                    //Open transaction
                    command.Transaction = this.Connection.BeginTransaction();

                    //Execute command
                    command.ExecuteNonQuery();

                    //Close transaction
                    command.Transaction.Commit();

                    //Close connection
                    this.Connection.Close();

                }
                catch (Exception ex) {

                    //Rollback transaction
                    command.Transaction.Rollback();

                    //Close connection
                    this.Connection.Close();

                    //Propagate exception
                    throw new RDFStoreException("Cannot delete data from SQLite store because: " + ex.Message, ex);

                }

            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context
        /// </summary>
        public override RDFStore RemoveQuadruplesByContext(RDFContext contextResource) {
            if (contextResource != null) {

                //Create command
                var command      = new SQLiteCommand("DELETE FROM main.Quadruples WHERE ContextID = @CTXID", this.Connection);
                command.Parameters.Add(new SQLiteParameter("CTXID", DbType.Int64));

                //Valorize parameters
                command.Parameters["CTXID"].Value = contextResource.PatternMemberID;

                try {

                    //Open connection
                    this.Connection.Open();

                    //Prepare command
                    command.Prepare();

                    //Open transaction
                    command.Transaction = this.Connection.BeginTransaction();

                    //Execute command
                    command.ExecuteNonQuery();

                    //Close transaction
                    command.Transaction.Commit();

                    //Close connection
                    this.Connection.Close();

                }
                catch (Exception ex) {

                    //Rollback transaction
                    command.Transaction.Rollback();

                    //Close connection
                    this.Connection.Close();

                    //Propagate exception
                    throw new RDFStoreException("Cannot delete data from SQLite store because: " + ex.Message, ex);

                }

            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given subject
        /// </summary>
        public override RDFStore RemoveQuadruplesBySubject(RDFResource subjectResource) {
            if (subjectResource != null) {

                //Create command
                var command      = new SQLiteCommand("DELETE FROM main.Quadruples WHERE SubjectID = @SUBJID", this.Connection);
                command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));

                //Valorize parameters
                command.Parameters["SUBJID"].Value = subjectResource.PatternMemberID;

                try {

                    //Open connection
                    this.Connection.Open();

                    //Prepare command
                    command.Prepare();

                    //Open transaction
                    command.Transaction = this.Connection.BeginTransaction();

                    //Execute command
                    command.ExecuteNonQuery();

                    //Close transaction
                    command.Transaction.Commit();

                    //Close connection
                    this.Connection.Close();

                }
                catch (Exception ex) {

                    //Rollback transaction
                    command.Transaction.Rollback();

                    //Close connection
                    this.Connection.Close();

                    //Propagate exception
                    throw new RDFStoreException("Cannot delete data from SQLite store because: " + ex.Message, ex);

                }

            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given predicate
        /// </summary>
        public override RDFStore RemoveQuadruplesByPredicate(RDFResource predicateResource) {
            if (predicateResource != null) {

                //Create command
                var command        = new SQLiteCommand("DELETE FROM main.Quadruples WHERE PredicateID = @PREDID", this.Connection);
                command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));

                //Valorize parameters
                command.Parameters["PREDID"].Value = predicateResource.PatternMemberID;

                try {

                    //Open connection
                    this.Connection.Open();

                    //Prepare command
                    command.Prepare();

                    //Open transaction
                    command.Transaction = this.Connection.BeginTransaction();

                    //Execute command
                    command.ExecuteNonQuery();

                    //Close transaction
                    command.Transaction.Commit();

                    //Close connection
                    this.Connection.Close();

                }
                catch (Exception ex) {

                    //Rollback transaction
                    command.Transaction.Rollback();

                    //Close connection
                    this.Connection.Close();

                    //Propagate exception
                    throw new RDFStoreException("Cannot delete data from SQLite store because: " + ex.Message, ex);

                }

            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given resource as object
        /// </summary>
        public override RDFStore RemoveQuadruplesByObject(RDFResource objectResource) {
            if (objectResource   != null) {

                //Create command
                var command       = new SQLiteCommand("DELETE FROM main.Quadruples WHERE ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                command.Parameters.Add(new SQLiteParameter("OBJID", DbType.Int64));
                command.Parameters.Add(new SQLiteParameter("TFV",   DbType.Int32));

                //Valorize parameters
                command.Parameters["OBJID"].Value = objectResource.PatternMemberID;
                command.Parameters["TFV"].Value   = RDFModelEnums.RDFTripleFlavors.SPO;

                try {

                    //Open connection
                    this.Connection.Open();

                    //Prepare command
                    command.Prepare();

                    //Open transaction
                    command.Transaction = this.Connection.BeginTransaction();

                    //Execute command
                    command.ExecuteNonQuery();

                    //Close transaction
                    command.Transaction.Commit();

                    //Close connection
                    this.Connection.Close();

                }
                catch (Exception ex) {

                    //Rollback transaction
                    command.Transaction.Rollback();

                    //Close connection
                    this.Connection.Close();

                    //Propagate exception
                    throw new RDFStoreException("Cannot delete data from SQLite store because: " + ex.Message, ex);

                }

            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given literal as object
        /// </summary>
        public override RDFStore RemoveQuadruplesByLiteral(RDFLiteral literalObject) {
            if (literalObject    != null) {

                //Create command
                var command       = new SQLiteCommand("DELETE FROM main.Quadruples WHERE ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                command.Parameters.Add(new SQLiteParameter("OBJID", DbType.Int64));
                command.Parameters.Add(new SQLiteParameter("TFV",   DbType.Int32));

                //Valorize parameters
                command.Parameters["OBJID"].Value = literalObject.PatternMemberID;
                command.Parameters["TFV"].Value   = RDFModelEnums.RDFTripleFlavors.SPL;

                try {

                    //Open connection
                    this.Connection.Open();

                    //Prepare command
                    command.Prepare();

                    //Open transaction
                    command.Transaction = this.Connection.BeginTransaction();

                    //Execute command
                    command.ExecuteNonQuery();

                    //Close transaction
                    command.Transaction.Commit();

                    //Close connection
                    this.Connection.Close();

                }
                catch (Exception ex) {

                    //Rollback transaction
                    command.Transaction.Rollback();

                    //Close connection
                    this.Connection.Close();

                    //Propagate exception
                    throw new RDFStoreException("Cannot delete data from SQLite store because: " + ex.Message, ex);

                }

            }
            return this;
        }

        /// <summary>
        /// Clears the quadruples of the store
        /// </summary>
        public override RDFStore ClearQuadruples() {

            //Create command
            var command = new SQLiteCommand("DELETE FROM main.Quadruples", this.Connection);

            try {

                //Open connection
                this.Connection.Open();

                //Prepare command
                command.Prepare();

                //Open transaction
                command.Transaction = this.Connection.BeginTransaction();

                //Execute command
                command.ExecuteNonQuery();

                //Close transaction
                command.Transaction.Commit();

                //Close connection
                this.Connection.Close();

            }
            catch (Exception ex) {

                //Rollback transaction
                command.Transaction.Rollback();

                //Close connection
                this.Connection.Close();

                //Propagate exception
                throw new RDFStoreException("Cannot delete data from SQLite store because: " + ex.Message, ex);

            }

            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Checks if the store contains the given quadruple
        /// </summary>
        public override Boolean ContainsQuadruple(RDFQuadruple quadruple) {
            if (quadruple   != null) {
                if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                    return (this.SelectQuadruples((RDFContext)quadruple.Context,
                                                  (RDFResource)quadruple.Subject,
                                                  (RDFResource)quadruple.Predicate,
                                                  (RDFResource)quadruple.Object,
                                                  null)).QuadruplesCount > 0;
                }
                else {
                    return (this.SelectQuadruples((RDFContext)quadruple.Context,
                                                  (RDFResource)quadruple.Subject,
                                                  (RDFResource)quadruple.Predicate,
                                                  null,
                                                  (RDFLiteral)quadruple.Object)).QuadruplesCount > 0;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a store containing all quadruples
        /// </summary>
        public override RDFMemoryStore SelectAllQuadruples() {
            return this.SelectQuadruples(null, null, null, null, null);
        }

        /// <summary>
        /// Gets a memory store containing quadruples with the specified context
        /// </summary>
        public override RDFMemoryStore SelectQuadruplesByContext(RDFContext contextResource) {
            return this.SelectQuadruples(contextResource, null, null, null, null);
        }

        /// <summary>
        /// Gets a memory store containing quadruples with the specified subject
        /// </summary>
        public override RDFMemoryStore SelectQuadruplesBySubject(RDFResource subjectResource) {
            return this.SelectQuadruples(null, subjectResource, null, null, null);
        }

        /// <summary>
        /// Gets a memory store containing quadruples with the specified predicate
        /// </summary>
        public override RDFMemoryStore SelectQuadruplesByPredicate(RDFResource predicateResource) {
            return this.SelectQuadruples(null, null, predicateResource, null, null);
        }

        /// <summary>
        /// Gets a memory store containing quadruples with the specified object
        /// </summary>
        public override RDFMemoryStore SelectQuadruplesByObject(RDFResource objectResource) {
            return this.SelectQuadruples(null, null, null, objectResource, null);
        }

        /// <summary>
        /// Gets a memory store containing quadruples with the specified literal
        /// </summary>
        public override RDFMemoryStore SelectQuadruplesByLiteral(RDFLiteral objectLiteral) {
            return this.SelectQuadruples(null, null, null, null, objectLiteral);
        }

        /// <summary>
        /// Gets a memory store containing quadruples satisfying the given pattern
        /// </summary>
        internal override RDFMemoryStore SelectQuadruples(RDFContext  ctx,
                                                          RDFResource subj,
                                                          RDFResource pred,
                                                          RDFResource obj,
                                                          RDFLiteral  lit) {
            RDFMemoryStore result    = new RDFMemoryStore();
            SQLiteCommand command    = null;

            //Intersect the filters
            if (ctx                 != null) {
                if (subj            != null) {
                    if (pred        != null) {
                        if (obj     != null) {
                            //C->S->P->O
                            command  = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID AND SubjectID = @SUBJID AND PredicateID = @PREDID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                            command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                            command.Parameters.Add(new SQLiteParameter("CTXID",  DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                            command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPO;
                            command.Parameters["CTXID"].Value  = ctx.PatternMemberID;
                            command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                            command.Parameters["PREDID"].Value = pred.PatternMemberID;
                            command.Parameters["OBJID"].Value  = obj.PatternMemberID;
                        }
                        else {
                            if (lit != null) {
                                //C->S->P->L
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID AND SubjectID = @SUBJID AND PredicateID = @PREDID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                                command.Parameters.Add(new SQLiteParameter("CTXID",  DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                                command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPL;
                                command.Parameters["CTXID"].Value  = ctx.PatternMemberID;
                                command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                                command.Parameters["PREDID"].Value = pred.PatternMemberID;
                                command.Parameters["OBJID"].Value  = lit.PatternMemberID;
                            }
                            else {
                                //C->S->P->
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID AND SubjectID = @SUBJID AND PredicateID = @PREDID", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("CTXID",  DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                                command.Parameters["CTXID"].Value  = ctx.PatternMemberID;
                                command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                                command.Parameters["PREDID"].Value = pred.PatternMemberID;
                            }
                        }
                    }
                    else {
                        if (obj     != null) {
                            //C->S->->O
                            command  = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID AND SubjectID = @SUBJID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                            command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                            command.Parameters.Add(new SQLiteParameter("CTXID",  DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                            command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPO;
                            command.Parameters["CTXID"].Value  = ctx.PatternMemberID;
                            command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                            command.Parameters["OBJID"].Value  = obj.PatternMemberID;
                        }
                        else {
                            if (lit    != null) {
                                //C->S->->L
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID AND SubjectID = @SUBJID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                                command.Parameters.Add(new SQLiteParameter("CTXID",  DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                                command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPL;
                                command.Parameters["CTXID"].Value  = ctx.PatternMemberID;
                                command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                                command.Parameters["OBJID"].Value  = lit.PatternMemberID;
                            }
                            else {
                                //C->S->->
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID AND SubjectID = @SUBJID", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("CTXID",  DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                                command.Parameters["CTXID"].Value  = ctx.PatternMemberID;
                                command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                            }
                        }
                    }
                }
                else {
                    if (pred        != null) {
                        if (obj     != null) {
                            //C->->P->O
                            command  = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID AND PredicateID = @PREDID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                            command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                            command.Parameters.Add(new SQLiteParameter("CTXID",  DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                            command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPO;
                            command.Parameters["CTXID"].Value  = ctx.PatternMemberID;
                            command.Parameters["PREDID"].Value = pred.PatternMemberID;
                            command.Parameters["OBJID"].Value  = obj.PatternMemberID;
                        }
                        else {
                            if (lit != null) {
                                //C->->P->L
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID AND PredicateID = @PREDID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                                command.Parameters.Add(new SQLiteParameter("CTXID",  DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                                command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPL;
                                command.Parameters["CTXID"].Value  = ctx.PatternMemberID;
                                command.Parameters["PREDID"].Value = pred.PatternMemberID;
                                command.Parameters["OBJID"].Value  = lit.PatternMemberID;
                            }
                            else {
                                //C->->P->
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID AND PredicateID = @PREDID", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("CTXID",  DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                                command.Parameters["CTXID"].Value  = ctx.PatternMemberID;
                                command.Parameters["PREDID"].Value = pred.PatternMemberID;
                            }
                        }
                    }
                    else {
                        if (obj     != null) {
                            //C->->->O
                            command  = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                            command.Parameters.Add(new SQLiteParameter("TFV",   DbType.Int32));
                            command.Parameters.Add(new SQLiteParameter("CTXID", DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("OBJID", DbType.Int64));
                            command.Parameters["TFV"].Value   = RDFModelEnums.RDFTripleFlavors.SPO;
                            command.Parameters["CTXID"].Value = ctx.PatternMemberID;
                            command.Parameters["OBJID"].Value = obj.PatternMemberID;
                        }
                        else {
                            if (lit != null) {
                                //C->->->L
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("TFV",   DbType.Int32));
                                command.Parameters.Add(new SQLiteParameter("CTXID", DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("OBJID", DbType.Int64));
                                command.Parameters["TFV"].Value   = RDFModelEnums.RDFTripleFlavors.SPL;
                                command.Parameters["CTXID"].Value = ctx.PatternMemberID;
                                command.Parameters["OBJID"].Value = lit.PatternMemberID;
                            }
                            else {
                                //C->->->
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ContextID = @CTXID", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("CTXID", DbType.Int64));
                                command.Parameters["CTXID"].Value = ctx.PatternMemberID;
                            }
                        }
                    }
                }
            }
            else {
                if (subj            != null) {
                    if (pred        != null) {
                        if (obj     != null) {
                            //->S->P->O
                            command  = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE SubjectID = @SUBJID AND PredicateID = @PREDID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                            command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                            command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                            command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPO;
                            command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                            command.Parameters["PREDID"].Value = pred.PatternMemberID;
                            command.Parameters["OBJID"].Value  = obj.PatternMemberID;
                        }
                        else {
                            if (lit != null) {
                                //->S->P->L
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE SubjectID = @SUBJID AND PredicateID = @PREDID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                                command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                                command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPL;
                                command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                                command.Parameters["PREDID"].Value = pred.PatternMemberID;
                                command.Parameters["OBJID"].Value  = lit.PatternMemberID;
                            }
                            else {
                                //->S->P->
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE SubjectID = @SUBJID AND PredicateID = @PREDID", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                                command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                                command.Parameters["PREDID"].Value = pred.PatternMemberID;
                            }
                        }
                    }
                    else {
                        if (obj     != null) {
                            //->S->->O
                            command  = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE SubjectID = @SUBJID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                            command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                            command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                            command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPO;
                            command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                            command.Parameters["OBJID"].Value  = obj.PatternMemberID;
                        }
                        else {
                            if (lit != null) {
                                //->S->->L
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE SubjectID = @SUBJID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                                command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                                command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPL;
                                command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                                command.Parameters["OBJID"].Value  = lit.PatternMemberID;
                            }
                            else {
                                //->S->->
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE SubjectID = @SUBJID", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("SUBJID", DbType.Int64));
                                command.Parameters["SUBJID"].Value = subj.PatternMemberID;
                            }
                        }
                    }
                }
                else {
                    if (pred        != null) {
                        if (obj     != null) {
                            //->->P->O
                            command  = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE PredicateID = @PREDID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                            command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                            command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                            command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                            command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPO;
                            command.Parameters["PREDID"].Value = pred.PatternMemberID;
                            command.Parameters["OBJID"].Value  = obj.PatternMemberID;
                        }
                        else {
                            if (lit != null) {
                                //->->P->L
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE PredicateID = @PREDID AND ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("TFV",    DbType.Int32));
                                command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                                command.Parameters.Add(new SQLiteParameter("OBJID",  DbType.Int64));
                                command.Parameters["TFV"].Value    = RDFModelEnums.RDFTripleFlavors.SPL;
                                command.Parameters["PREDID"].Value = pred.PatternMemberID;
                                command.Parameters["OBJID"].Value  = lit.PatternMemberID;
                            }
                            else {
                                //->->P->
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE PredicateID = @PREDID", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("PREDID", DbType.Int64));
                                command.Parameters["PREDID"].Value = pred.PatternMemberID;
                            }
                        }
                    }
                    else {
                        if (obj     != null) {
                            //->->->O
                            command  = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                            command.Parameters.Add(new SQLiteParameter("TFV",   DbType.Int32));
                            command.Parameters.Add(new SQLiteParameter("OBJID", DbType.Int64));
                            command.Parameters["TFV"].Value   = RDFModelEnums.RDFTripleFlavors.SPO;
                            command.Parameters["OBJID"].Value = obj.PatternMemberID;
                        }
                        else {
                            if (lit != null) {
                                //->->->L
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples WHERE ObjectID = @OBJID AND TripleFlavor = @TFV", this.Connection);
                                command.Parameters.Add(new SQLiteParameter("TFV",   DbType.Int32));
                                command.Parameters.Add(new SQLiteParameter("OBJID", DbType.Int64));
                                command.Parameters["TFV"].Value   = RDFModelEnums.RDFTripleFlavors.SPL;
                                command.Parameters["OBJID"].Value = lit.PatternMemberID;
                            }
                            else {
                                //->->->
                                command = new SQLiteCommand("SELECT TripleFlavor, Context, Subject, Predicate, Object FROM main.Quadruples", this.Connection);
                            }
                        }
                    }
                }
            }

            //Prepare and execute command
            try {

                //Open connection
                this.Connection.Open();

                //Prepare command
                command.Prepare();

                //Execute command
                using (var quadruples = command.ExecuteReader()) {
                    if(quadruples.HasRows) {
                        while(quadruples.Read()) {
                            result.AddQuadruple(RDFStoreUtilities.ParseQuadruple(quadruples));
                        }
                    }
                }

                //Close connection
                this.Connection.Close();

            }
            catch (Exception ex) {

                //Close connection
                this.Connection.Close();

                //Propagate exception
                throw new RDFStoreException("Cannot read data from SQLite store because: " + ex.Message, ex);

            }

            return result;
        }
        #endregion

        #region Diagnostics
        /// <summary>
        /// Performs the preliminary diagnostics controls on the underlying SQLite database
        /// </summary>
        private RDFStoreEnums.RDFStoreSQLErrors Diagnostics() {
            try {

                //Open connection
                this.Connection.Open();

                //Create command
                var command     = new SQLiteCommand("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Quadruples'", this.Connection);

                //Execute command
                var result      = Int32.Parse(command.ExecuteScalar().ToString());

                //Close connection
                this.Connection.Close();

                //Return the diagnostics state
                return (result == 0 ? RDFStoreEnums.RDFStoreSQLErrors.QuadruplesTableNotFound : RDFStoreEnums.RDFStoreSQLErrors.NoErrors);

            }
            catch {

                //Close connection
                this.Connection.Close();

                //Return the diagnostics state
                return RDFStoreEnums.RDFStoreSQLErrors.InvalidDataSource;

            }
        }

        /// <summary>
        /// Prepares the underlying SQLite database
        /// </summary>
        private void PrepareStore() {
            var check           = this.Diagnostics();

            //Prepare the database only if diagnostics has detected the missing of "Quadruples" table in the store
            if (check          == RDFStoreEnums.RDFStoreSQLErrors.QuadruplesTableNotFound) {
                try {

                    //Open connection
                    this.Connection.Open();

                    //Create command
                    var command = new SQLiteCommand("CREATE TABLE main.Quadruples (QuadrupleID INTEGER NOT NULL PRIMARY KEY, TripleFlavor INTEGER NOT NULL, Context VARCHAR(1000) NOT NULL, ContextID INTEGER NOT NULL, Subject VARCHAR(1000) NOT NULL, SubjectID INTEGER NOT NULL, Predicate VARCHAR(1000) NOT NULL, PredicateID INTEGER NOT NULL, Object VARCHAR(5000) NOT NULL, ObjectID INTEGER NOT NULL);CREATE INDEX IDX_ContextID ON main.Quadruples (ContextID);CREATE INDEX IDX_SubjectID ON main.Quadruples (SubjectID);CREATE INDEX IDX_PredicateID ON main.Quadruples (PredicateID);CREATE INDEX IDX_ObjectID ON main.Quadruples (ObjectID,TripleFlavor);CREATE INDEX IDX_SubjectID_PredicateID ON main.Quadruples (SubjectID,PredicateID);CREATE INDEX IDX_SubjectID_ObjectID ON main.Quadruples (SubjectID,ObjectID,TripleFlavor);CREATE INDEX IDX_PredicateID_ObjectID ON main.Quadruples (PredicateID,ObjectID,TripleFlavor);", this.Connection);

                    //Execute command
                    command.ExecuteNonQuery();

                    //Close connection
                    this.Connection.Close();

                }
                catch (Exception ex) {

                    //Close connection
                    this.Connection.Close();

                    //Propagate exception
                    throw new RDFStoreException("Cannot prepare SQLite store because: " + ex.Message, ex);

                }
            }

            //Otherwise, an exception must be thrown because it has not been possible to connect to the database
            else if (check     == RDFStoreEnums.RDFStoreSQLErrors.InvalidDataSource) {
                throw new RDFStoreException("Cannot prepare SQLite store because: unable to open the database.");
            }
        }
        #endregion		

        #region Optimize
        /// <summary>
        /// Executes a special command to optimize SQLite store
        /// </summary>
        public void OptimizeStore() {
            try {

                //Open connection
                this.Connection.Open();

                //Create command
                var command = new SQLiteCommand("VACUUM", this.Connection);

                //Execute command
                command.ExecuteNonQuery();

                //Close connection
                this.Connection.Close();

            }
            catch (Exception ex) {

                //Close connection
                this.Connection.Close();

                //Propagate exception
                throw new RDFStoreException("Cannot optimize SQLite store because: " + ex.Message, ex);

            }
        }
        #endregion

        #endregion

    }

}