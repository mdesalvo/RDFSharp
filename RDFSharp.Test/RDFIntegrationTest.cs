/*
   Copyright 2012-2026 Marco De Salvo

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
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test;

/// <summary>
/// End-to-end integration tests for the Mirella query engine. The 15 queries cut across the whole
/// engine surface: BGP, multi-join, OPTIONAL, UNION, FILTER, GROUP BY/aggregation+HAVING,
/// ORDER BY+LIMIT, DISTINCT, transitive property paths, sub-query, projection expression (BIND-like),
/// FILTER NOT EXISTS, FILTER (multi-pattern) EXISTS, inline VALUES and deeply nested Union/Minus operator trees.
/// </summary>
[TestClass]
public class RDFIntegrationTest
{
    #region Test data
    /// <summary>
    /// Base namespace of the university test dataset (same one used by the benchmark suite)
    /// </summary>
    private const string UniversityNamespace = "http://uni.org/";

    /// <summary>
    /// The shared university graph: built once for the whole test class, since every test only reads it
    /// </summary>
    private static RDFGraph UniversityGraph;

    /// <summary>
    /// Builds a resource living in the university namespace
    /// </summary>
    private static RDFResource UniversityResource(string localName)
        => new RDFResource(UniversityNamespace + localName);

    /// <summary>
    /// Builds a SPARQL variable with the given name
    /// </summary>
    private static RDFVariable Variable(string variableName)
        => new RDFVariable(variableName);

    /// <summary>
    /// Builds an xsd:integer typed literal with the given value
    /// </summary>
    private static RDFTypedLiteral IntegerLiteral(string value)
        => new RDFTypedLiteral(value, RDFModelEnums.RDFDatatypes.XSD_INTEGER);

    /// <summary>
    /// Builds an english plain literal with the given value
    /// </summary>
    private static RDFPlainLiteral EnglishLiteral(string value)
        => new RDFPlainLiteral(value, "en");

    /// <summary>
    /// Extracts the numeric value carried by a result cell containing a typed literal
    /// (e.g. "26^^http://www.w3.org/2001/XMLSchema#decimal" => 26)
    /// </summary>
    private static double GetNumericValue(object resultCell)
    {
        string cellValue = resultCell.ToString();
        int datatypeSeparatorIndex = cellValue.IndexOf("^^", StringComparison.Ordinal);
        string numericPart = datatypeSeparatorIndex >= 0 ? cellValue.Substring(0, datatypeSeparatorIndex) : cellValue;
        return double.Parse(numericPart, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Asserts that the query prints exactly the expected SPARQL string, then applies it to the
    /// university graph. Both operands are EOL-normalized because the printer emits
    /// Environment.NewLine while the expected strings in this file are stored with LF.
    /// </summary>
    private static RDFSelectQueryResult AssertQueryStringAndApply(string expectedQueryString, RDFSelectQuery query)
    {
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(expectedQueryString), RDFTestUtilities.NormalizeEOL(query.ToString()));
        return query.ApplyToGraph(UniversityGraph);
    }

    /// <summary>
    /// Builds the deterministic mini-university graph against which every query is evaluated.
    ///
    /// Shape of the dataset (kept intentionally small so each test can be verified by hand):
    /// - 2 buildings (building0, building1) with name and capacity;
    /// - 2 professors (prof0 works in building0, prof1 in building1) with name and age;
    /// - 4 courses: course0/course1 taught by prof0 in building0, course2 taught by prof1 in building1,
    ///   course3 taught by prof1 WITHOUT a building (exercises OPTIONAL);
    ///   prerequisite chain: course1->course0, course2->course1, course3->course1 (exercises property paths);
    /// - 4 students: student0 (age 20, enrolled in course0+course1), student1 (age 26, course1+course2),
    ///   student2 (age 30, course2+course3), student3 (age 24, course0, NO exams => exercises NOT EXISTS);
    /// - 5 exams: course0 graded [25], course1 graded [28,18], course2 graded [30,22]
    ///   => per-course averages: course0=25, course1=23, course2=26 (exercises GROUP BY/HAVING/sub-query).
    /// </summary>
    [ClassInitialize]
    public static void InitializeUniversityGraph(TestContext testContext)
    {
        UniversityGraph = new RDFGraph();

        //Buildings
        for (int buildingIndex = 0; buildingIndex < 2; buildingIndex++)
        {
            RDFResource building = UniversityResource($"building{buildingIndex}");
            UniversityGraph.AddTriple(new RDFTriple(building, RDFVocabulary.RDF.TYPE, UniversityResource("Building")));
            UniversityGraph.AddTriple(new RDFTriple(building, UniversityResource("name"), EnglishLiteral($"Building {buildingIndex}")));
            UniversityGraph.AddTriple(new RDFTriple(building, UniversityResource("capacity"), IntegerLiteral(buildingIndex == 0 ? "100" : "200")));
        }

        //Professors
        for (int professorIndex = 0; professorIndex < 2; professorIndex++)
        {
            RDFResource professor = UniversityResource($"prof{professorIndex}");
            UniversityGraph.AddTriple(new RDFTriple(professor, RDFVocabulary.RDF.TYPE, UniversityResource("Professor")));
            UniversityGraph.AddTriple(new RDFTriple(professor, UniversityResource("name"), EnglishLiteral($"Professor {professorIndex}")));
            UniversityGraph.AddTriple(new RDFTriple(professor, UniversityResource("worksIn"), UniversityResource($"building{professorIndex}")));
            UniversityGraph.AddTriple(new RDFTriple(professor, UniversityResource("age"), IntegerLiteral(professorIndex == 0 ? "50" : "60")));
        }

        //Courses (course3 has no building on purpose; prerequisite chain course3/course2 -> course1 -> course0)
        (string professor, string building, string credits, string prerequisite)[] courseDescriptors =
        [
            ("prof0", "building0", "6",  null),
            ("prof0", "building0", "9",  "course0"),
            ("prof1", "building1", "6",  "course1"),
            ("prof1", null,        "12", "course1")
        ];
        for (int courseIndex = 0; courseIndex < courseDescriptors.Length; courseIndex++)
        {
            RDFResource course = UniversityResource($"course{courseIndex}");
            UniversityGraph.AddTriple(new RDFTriple(course, RDFVocabulary.RDF.TYPE, UniversityResource("Course")));
            UniversityGraph.AddTriple(new RDFTriple(course, UniversityResource("name"), EnglishLiteral($"Course {courseIndex}")));
            UniversityGraph.AddTriple(new RDFTriple(course, UniversityResource("taughtBy"), UniversityResource(courseDescriptors[courseIndex].professor)));
            UniversityGraph.AddTriple(new RDFTriple(course, UniversityResource("credits"), IntegerLiteral(courseDescriptors[courseIndex].credits)));
            if (courseDescriptors[courseIndex].building != null)
                UniversityGraph.AddTriple(new RDFTriple(course, UniversityResource("locatedIn"), UniversityResource(courseDescriptors[courseIndex].building)));
            if (courseDescriptors[courseIndex].prerequisite != null)
                UniversityGraph.AddTriple(new RDFTriple(course, UniversityResource("prerequisite"), UniversityResource(courseDescriptors[courseIndex].prerequisite)));
        }

        //Students with their enrollments (student3 will have no exams on purpose)
        (string age, string[] courses)[] studentDescriptors =
        [
            ("20", ["course0", "course1"]),
            ("26", ["course1", "course2"]),
            ("30", ["course2", "course3"]),
            ("24", ["course0"])
        ];
        for (int studentIndex = 0; studentIndex < studentDescriptors.Length; studentIndex++)
        {
            RDFResource student = UniversityResource($"student{studentIndex}");
            UniversityGraph.AddTriple(new RDFTriple(student, RDFVocabulary.RDF.TYPE, UniversityResource("Student")));
            UniversityGraph.AddTriple(new RDFTriple(student, UniversityResource("name"), EnglishLiteral($"Student {studentIndex}")));
            UniversityGraph.AddTriple(new RDFTriple(student, UniversityResource("age"), IntegerLiteral(studentDescriptors[studentIndex].age)));
            foreach (string enrolledCourse in studentDescriptors[studentIndex].courses)
                UniversityGraph.AddTriple(new RDFTriple(student, UniversityResource("enrolledIn"), UniversityResource(enrolledCourse)));
        }

        //Exams (student3 deliberately left without exams; per-course averages: course0=25, course1=23, course2=26)
        (string student, string course, string grade, string date)[] examDescriptors =
        [
            ("student0", "course0", "25", "2024-06-15"),
            ("student0", "course1", "28", "2024-07-01"),
            ("student1", "course1", "18", "2025-01-20"),
            ("student1", "course2", "30", "2025-02-10"),
            ("student2", "course2", "22", "2025-06-05")
        ];
        for (int examIndex = 0; examIndex < examDescriptors.Length; examIndex++)
        {
            RDFResource exam = UniversityResource($"exam{examIndex}");
            UniversityGraph.AddTriple(new RDFTriple(exam, RDFVocabulary.RDF.TYPE, UniversityResource("Exam")));
            UniversityGraph.AddTriple(new RDFTriple(exam, UniversityResource("examStudent"), UniversityResource(examDescriptors[examIndex].student)));
            UniversityGraph.AddTriple(new RDFTriple(exam, UniversityResource("examCourse"), UniversityResource(examDescriptors[examIndex].course)));
            UniversityGraph.AddTriple(new RDFTriple(exam, UniversityResource("grade"), IntegerLiteral(examDescriptors[examIndex].grade)));
            UniversityGraph.AddTriple(new RDFTriple(exam, UniversityResource("examDate"), new RDFTypedLiteral(examDescriptors[examIndex].date, RDFModelEnums.RDFDatatypes.XSD_DATE)));
        }
    }
    #endregion

    #region Tests
    [TestMethod]
    public void ShouldAnswerQ01BasicGraphPatternOnStudents()
    {
        //Q01 - simple BGP (type + name join over all students)
        const string expectedQueryString = @"SELECT *
WHERE {
  {
    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Student> .
    ?S <http://uni.org/name> ?N .
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?s"), RDFVocabulary.RDF.TYPE, UniversityResource("Student")))
                .AddPattern(new RDFPattern(Variable("?s"), UniversityResource("name"), Variable("?n")))));

        //Every one of the 4 students has exactly one name
        Assert.AreEqual(4, result.SelectResultsCount);

        //Each student must come back paired with its own name
        foreach (DataRow resultRow in result.SelectResults.Rows)
        {
            string studentIndex = resultRow["?S"].ToString().Substring($"{UniversityNamespace}student".Length);
            Assert.AreEqual($"Student {studentIndex}@EN", resultRow["?N"].ToString());
        }
    }

    [TestMethod]
    public void ShouldAnswerQ02FivePatternJoinStudentCourseProfessor()
    {
        //Q02 - 5-pattern join (student->course->prof->name + course->building)
        const string expectedQueryString = @"SELECT *
WHERE {
  {
    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Student> .
    ?S <http://uni.org/enrolledIn> ?C .
    ?C <http://uni.org/taughtBy> ?P .
    ?P <http://uni.org/name> ?PN .
    ?C <http://uni.org/locatedIn> ?B .
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?s"), RDFVocabulary.RDF.TYPE, UniversityResource("Student")))
                .AddPattern(new RDFPattern(Variable("?s"), UniversityResource("enrolledIn"), Variable("?c")))
                .AddPattern(new RDFPattern(Variable("?c"), UniversityResource("taughtBy"), Variable("?p")))
                .AddPattern(new RDFPattern(Variable("?p"), UniversityResource("name"), Variable("?pn")))
                .AddPattern(new RDFPattern(Variable("?c"), UniversityResource("locatedIn"), Variable("?b")))));

        //7 enrollments in total, but student2->course3 drops out because course3 has no building => 6 rows
        Assert.AreEqual(6, result.SelectResultsCount);

        //Professor names must be consistent with the taughtBy assignments (course0/1 => prof0, course2 => prof1)
        foreach (DataRow resultRow in result.SelectResults.Rows)
        {
            string courseUri = resultRow["?C"].ToString();
            string expectedProfessorName = courseUri.EndsWith("course2", StringComparison.Ordinal) ? "Professor 1@EN" : "Professor 0@EN";
            Assert.AreEqual(expectedProfessorName, resultRow["?PN"].ToString());
        }
    }

    [TestMethod]
    public void ShouldAnswerQ03OptionalCourseBuilding()
    {
        //Q03 - OPTIONAL (course name, optional building)
        const string expectedQueryString = @"SELECT *
WHERE {
  {
    ?C <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Course> .
    ?C <http://uni.org/name> ?CN .
    OPTIONAL { ?C <http://uni.org/locatedIn> ?B } .
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?c"), RDFVocabulary.RDF.TYPE, UniversityResource("Course")))
                .AddPattern(new RDFPattern(Variable("?c"), UniversityResource("name"), Variable("?cn")))
                .AddPattern(new RDFPattern(Variable("?c"), UniversityResource("locatedIn"), Variable("?b")).Optional())));

        //All 4 courses survive the optional join; only course3 has an unbound building
        Assert.AreEqual(4, result.SelectResultsCount);
        foreach (DataRow resultRow in result.SelectResults.Rows)
        {
            bool isCourse3 = resultRow["?C"].ToString().EndsWith("course3", StringComparison.Ordinal);
            Assert.AreEqual(isCourse3, string.IsNullOrEmpty(resultRow["?B"].ToString()));
        }
    }

    [TestMethod]
    public void ShouldAnswerQ04UnionOfStudentsAndProfessors()
    {
        //Q04 - UNION (students UNION professors, with names), expressed with the v4 tree-based operator
        RDFPatternGroup studentsPatternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(Variable("?x"), RDFVocabulary.RDF.TYPE, UniversityResource("Student")))
            .AddPattern(new RDFPattern(Variable("?x"), UniversityResource("name"), Variable("?n")));
        RDFPatternGroup professorsPatternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(Variable("?x"), RDFVocabulary.RDF.TYPE, UniversityResource("Professor")))
            .AddPattern(new RDFPattern(Variable("?x"), UniversityResource("name"), Variable("?n")));
        const string expectedQueryString = @"SELECT *
WHERE {
  {
    {
      ?X <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Student> .
      ?X <http://uni.org/name> ?N .
    }
    UNION
    {
      ?X <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Professor> .
      ?X <http://uni.org/name> ?N .
    }
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddBinaryQueryMember(studentsPatternGroup.Union(professorsPatternGroup)));

        //4 students + 2 professors = 6 people
        Assert.AreEqual(6, result.SelectResultsCount);

        //The union must contain both branches: count rows coming from each one
        int studentRowCount = 0, professorRowCount = 0;
        foreach (DataRow resultRow in result.SelectResults.Rows)
        {
            if (resultRow["?N"].ToString().StartsWith("Student", StringComparison.Ordinal)) studentRowCount++;
            if (resultRow["?N"].ToString().StartsWith("Professor", StringComparison.Ordinal)) professorRowCount++;
        }
        Assert.AreEqual(4, studentRowCount);
        Assert.AreEqual(2, professorRowCount);
    }

    [TestMethod]
    public void ShouldAnswerQ05FilterStudentsOlderThan25()
    {
        //Q05 - FILTER (numeric comparison on age)
        const string expectedQueryString = @"SELECT *
WHERE {
  {
    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Student> .
    ?S <http://uni.org/age> ?A .
    FILTER ( (?A > 25) ) 
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?s"), RDFVocabulary.RDF.TYPE, UniversityResource("Student")))
                .AddPattern(new RDFPattern(Variable("?s"), UniversityResource("age"), Variable("?a")))
                .AddFilter(new RDFExpressionFilter(new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                    new RDFVariableExpression(Variable("?a")),
                    new RDFConstantExpression(IntegerLiteral("25")))))));

        //Only student1 (age 26) and student2 (age 30) pass the "age > 25" filter
        Assert.AreEqual(2, result.SelectResultsCount);
        foreach (DataRow resultRow in result.SelectResults.Rows)
            Assert.IsTrue(GetNumericValue(resultRow["?A"]) > 25);
    }

    [TestMethod]
    public void ShouldAnswerQ06GroupByWithCountAvgAndHaving()
    {
        //Q06 - GROUP BY + COUNT/AVG + HAVING (per-course exam stats)
        const string expectedQueryString = @"SELECT ?C (COUNT(?E) AS ?CNT) (AVG(?G) AS ?AVG)
WHERE {
  {
    ?E <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Exam> .
    ?E <http://uni.org/examCourse> ?C .
    ?E <http://uni.org/grade> ?G .
  }
}
GROUP BY ?C
HAVING ((?AVG >= 24))
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?e"), RDFVocabulary.RDF.TYPE, UniversityResource("Exam")))
                .AddPattern(new RDFPattern(Variable("?e"), UniversityResource("examCourse"), Variable("?c")))
                .AddPattern(new RDFPattern(Variable("?e"), UniversityResource("grade"), Variable("?g"))))
            .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { Variable("?c") })
                .AddAggregator(new RDFCountAggregator(Variable("?e"), Variable("?cnt")))
                .AddAggregator(new RDFAvgAggregator(Variable("?g"), Variable("?avg")))
                .SetHavingExpression(new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                                                                 new RDFVariableExpression(Variable("?avg")),
                                                                 new RDFConstantExpression(new RDFTypedLiteral("24", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))))));

        //Per-course averages are course0=25, course1=23, course2=26: HAVING avg>=24 keeps course0 and course2
        Assert.AreEqual(2, result.SelectResultsCount);
        foreach (DataRow resultRow in result.SelectResults.Rows)
        {
            bool isCourse0 = resultRow["?C"].ToString().EndsWith("course0", StringComparison.Ordinal);
            Assert.AreEqual(isCourse0 ? 1 : 2, GetNumericValue(resultRow["?CNT"]));
            Assert.AreEqual(isCourse0 ? 25 : 26, GetNumericValue(resultRow["?AVG"]));
        }
    }

    [TestMethod]
    public void ShouldAnswerQ07OrderByAgeDescendingWithLimit()
    {
        //Q07 - ORDER BY + LIMIT (the 2 oldest students; the benchmark uses LIMIT 50,
        //here it is shrunk to 2 so the limit actually cuts the result set)
        const string expectedQueryString = @"SELECT *
WHERE {
  {
    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Student> .
    ?S <http://uni.org/age> ?A .
    ?S <http://uni.org/name> ?N .
  }
}
ORDER BY DESC(?A)
LIMIT 2
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?s"), RDFVocabulary.RDF.TYPE, UniversityResource("Student")))
                .AddPattern(new RDFPattern(Variable("?s"), UniversityResource("age"), Variable("?a")))
                .AddPattern(new RDFPattern(Variable("?s"), UniversityResource("name"), Variable("?n"))))
            .AddModifier(new RDFOrderByModifier(Variable("?a"), RDFQueryEnums.RDFOrderByFlavors.DESC))
            .AddModifier(new RDFLimitModifier(2)));

        //Ages are 20/26/30/24: descending order + limit 2 yields student2 (30) then student1 (26)
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.AreEqual("Student 2@EN", result.SelectResults.Rows[0]["?N"].ToString());
        Assert.AreEqual("Student 1@EN", result.SelectResults.Rows[1]["?N"].ToString());
    }

    [TestMethod]
    public void ShouldAnswerQ08DistinctBuildingsHostingCourses()
    {
        //Q08 - DISTINCT (distinct buildings hosting courses)
        const string expectedQueryString = @"SELECT DISTINCT ?B
WHERE {
  {
    ?C <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Course> .
    ?C <http://uni.org/locatedIn> ?B .
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?c"), RDFVocabulary.RDF.TYPE, UniversityResource("Course")))
                .AddPattern(new RDFPattern(Variable("?c"), UniversityResource("locatedIn"), Variable("?b"))))
            .AddProjectionVariable(Variable("?b"))
            .AddModifier(new RDFDistinctModifier()));

        //3 located courses collapse onto 2 distinct buildings (building0 hosts course0+course1)
        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldAnswerQ09TransitivePropertyPathOnPrerequisites()
    {
        //Q09 - property path (transitive prerequisite+ closure)
        const string expectedQueryString = @"SELECT *
WHERE {
  {
    ?C <http://uni.org/prerequisite>+ ?ROOT .
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Variable("?c"), Variable("?root"))
                    .AddSequenceStep(new RDFPropertyPathStep(UniversityResource("prerequisite")).OneOrMore()))));

        //Closure of "course1->course0, course2->course1, course3->course1":
        //course1+ = {course0}; course2+ = {course1, course0}; course3+ = {course1, course0} => 5 pairs
        Assert.AreEqual(5, result.SelectResultsCount);
        HashSet<string> reachedPairs = new HashSet<string>(StringComparer.Ordinal);
        foreach (DataRow resultRow in result.SelectResults.Rows)
            reachedPairs.Add($"{resultRow["?C"]}->{resultRow["?ROOT"]}");
        Assert.IsTrue(reachedPairs.Contains($"{UniversityNamespace}course1->{UniversityNamespace}course0"));
        Assert.IsTrue(reachedPairs.Contains($"{UniversityNamespace}course2->{UniversityNamespace}course1"));
        Assert.IsTrue(reachedPairs.Contains($"{UniversityNamespace}course2->{UniversityNamespace}course0"));
        Assert.IsTrue(reachedPairs.Contains($"{UniversityNamespace}course3->{UniversityNamespace}course1"));
        Assert.IsTrue(reachedPairs.Contains($"{UniversityNamespace}course3->{UniversityNamespace}course0"));
    }

    [TestMethod]
    public void ShouldAnswerQ10SubQueryWithPerCourseAverageGrade()
    {
        //Q10 - sub-query (per-course avg grade computed in subquery, joined to course name)
        RDFSelectQuery subQuery = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?e"), RDFVocabulary.RDF.TYPE, UniversityResource("Exam")))
                .AddPattern(new RDFPattern(Variable("?e"), UniversityResource("examCourse"), Variable("?c")))
                .AddPattern(new RDFPattern(Variable("?e"), UniversityResource("grade"), Variable("?g"))))
            .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { Variable("?c") })
                .AddAggregator(new RDFAvgAggregator(Variable("?g"), Variable("?avg"))));
        const string expectedQueryString = @"SELECT *
WHERE {
  {
    SELECT ?C (AVG(?G) AS ?AVG)
    WHERE {
      {
        ?E <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Exam> .
        ?E <http://uni.org/examCourse> ?C .
        ?E <http://uni.org/grade> ?G .
      }
    }
    GROUP BY ?C
  }
  {
    ?C <http://uni.org/name> ?CN .
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddSubQuery(subQuery)
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?c"), UniversityResource("name"), Variable("?cn")))));

        //Only course0/course1/course2 have exams; the outer join attaches their names
        Assert.AreEqual(3, result.SelectResultsCount);
        foreach (DataRow resultRow in result.SelectResults.Rows)
        {
            double expectedAverage;
            switch (resultRow["?CN"].ToString())
            {
                case "Course 0@EN": expectedAverage = 25; break;
                case "Course 1@EN": expectedAverage = 23; break;
                case "Course 2@EN": expectedAverage = 26; break;
                default: throw new AssertFailedException($"Unexpected course in sub-query results: {resultRow["?CN"]}");
            }
            Assert.AreEqual(expectedAverage, GetNumericValue(resultRow["?AVG"]));
        }
    }

    [TestMethod]
    public void ShouldAnswerQ11ProjectionExpressionOnGrades()
    {
        //Q11 - projection expression (BIND-like: grade + 1)
        const string expectedQueryString = @"SELECT ?G ((?G + 1) AS ?BONUS)
WHERE {
  {
    ?E <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Exam> .
    ?E <http://uni.org/grade> ?G .
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?e"), RDFVocabulary.RDF.TYPE, UniversityResource("Exam")))
                .AddPattern(new RDFPattern(Variable("?e"), UniversityResource("grade"), Variable("?g"))))
            .AddProjectionVariable(Variable("?g"))
            .AddProjectionVariable(Variable("?bonus"),
                new RDFAddExpression(new RDFVariableExpression(Variable("?g")), IntegerLiteral("1"))));

        //One row per exam (5), each carrying its grade incremented by 1 in the ?BONUS column
        Assert.AreEqual(5, result.SelectResultsCount);
        foreach (DataRow resultRow in result.SelectResults.Rows)
            Assert.AreEqual(GetNumericValue(resultRow["?G"]) + 1, GetNumericValue(resultRow["?BONUS"]));
    }

    [TestMethod]
    public void ShouldAnswerQ12FilterNotExistsStudentsWithoutExams()
    {
        //Q12 - FILTER NOT EXISTS (students with no exam)
        const string expectedQueryString = @"SELECT *
WHERE {
  {
    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Student> .
    FILTER ( NOT EXISTS { ?E <http://uni.org/examStudent> ?S . } ) 
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?s"), RDFVocabulary.RDF.TYPE, UniversityResource("Student")))
                .AddFilter(new RDFNotExistsFilter(new RDFPatternGroup().AddPattern(new RDFPattern(Variable("?e"), UniversityResource("examStudent"), Variable("?s")))))));

        //student3 is the only one who never took an exam
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.AreEqual($"{UniversityNamespace}student3", result.SelectResults.Rows[0]["?S"].ToString());
    }

    [TestMethod]
    public void ShouldAnswerQ13DeeplyNestedUnionMinusOperatorTree()
    {
        //Q13 - deeply nested tree-based operators: A.Union(B.Minus(C.Union(D))).Minus(E),
        //a shape that the v4 binary algebra tree enables and the old flag-based API could not express.
        //The operand sets over ?x are (computed by hand on the university dataset):
        // A = students                       => {student0, student1, student2, student3}
        // B = professors                     => {prof0, prof1}
        // C = people enrolled in course1     => {student0, student1}
        // D = people working in building0    => {prof0}
        // E = people enrolled in course2     => {student1, student2}
        //Evaluation, innermost first:
        // C.Union(D)               = {student0, student1, prof0}
        // B.Minus(C.Union(D))      = {prof1}                       (prof0 is removed, prof1 survives)
        // A.Union(...)             = {student0..student3, prof1}
        // (...).Minus(E)           = {student0, student3, prof1}   (student1 and student2 are removed)
        RDFPatternGroup studentsPatternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(Variable("?x"), RDFVocabulary.RDF.TYPE, UniversityResource("Student")))
            .AddPattern(new RDFPattern(Variable("?x"), UniversityResource("name"), Variable("?n")));
        RDFPatternGroup professorsPatternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(Variable("?x"), RDFVocabulary.RDF.TYPE, UniversityResource("Professor")))
            .AddPattern(new RDFPattern(Variable("?x"), UniversityResource("name"), Variable("?n")));
        RDFPatternGroup course1EnrolleesPatternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(Variable("?x"), UniversityResource("enrolledIn"), UniversityResource("course1")));
        RDFPatternGroup building0WorkersPatternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(Variable("?x"), UniversityResource("worksIn"), UniversityResource("building0")));
        RDFPatternGroup course2EnrolleesPatternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(Variable("?x"), UniversityResource("enrolledIn"), UniversityResource("course2")));

        const string expectedQueryString = @"SELECT *
WHERE {
  {
    {
      {
        ?X <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Student> .
        ?X <http://uni.org/name> ?N .
      }
      UNION
      {
        {
          ?X <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Professor> .
          ?X <http://uni.org/name> ?N .
        }
        MINUS
        {
          {
            ?X <http://uni.org/enrolledIn> <http://uni.org/course1> .
          }
          UNION
          {
            ?X <http://uni.org/worksIn> <http://uni.org/building0> .
          }
        }
      }
    }
    MINUS
    {
      ?X <http://uni.org/enrolledIn> <http://uni.org/course2> .
    }
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddBinaryQueryMember(studentsPatternGroup
                .Union(professorsPatternGroup
                    .Minus(course1EnrolleesPatternGroup
                        .Union(building0WorkersPatternGroup)))
                .Minus(course2EnrolleesPatternGroup)));

        //Survivors are student0, student3 and prof1, each carrying its own name
        Assert.AreEqual(3, result.SelectResultsCount);
        Dictionary<string, string> nameByPerson = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (DataRow resultRow in result.SelectResults.Rows)
            nameByPerson.Add(resultRow["?X"].ToString(), resultRow["?N"].ToString());
        Assert.AreEqual("Student 0@EN", nameByPerson[$"{UniversityNamespace}student0"]);
        Assert.AreEqual("Student 3@EN", nameByPerson[$"{UniversityNamespace}student3"]);
        Assert.AreEqual("Professor 1@EN", nameByPerson[$"{UniversityNamespace}prof1"]);
    }

    [TestMethod]
    public void ShouldAnswerQ14FilterExistsStudentsWithExamInProf0Course()
    {
        //Q14 - positive multi-pattern EXISTS (the new IP5.2 form): keep students for whom there EXISTS an exam
        //in a course taught by prof0. The EXISTS body is a 3-triple group correlated with the outer row on ?S.
        const string expectedQueryString = @"SELECT *
WHERE {
  {
    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Student> .
    FILTER ( EXISTS { ?EX <http://uni.org/examStudent> ?S . ?EX <http://uni.org/examCourse> ?COURSE . ?COURSE <http://uni.org/taughtBy> <http://uni.org/prof0> . } ) 
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(Variable("?s"), RDFVocabulary.RDF.TYPE, UniversityResource("Student")))
                .AddFilter(new RDFExistsFilter(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(Variable("?ex"), UniversityResource("examStudent"), Variable("?s")))
                    .AddPattern(new RDFPattern(Variable("?ex"), UniversityResource("examCourse"), Variable("?course")))
                    .AddPattern(new RDFPattern(Variable("?course"), UniversityResource("taughtBy"), UniversityResource("prof0")))))));

        //student0 (exam in course0+course1, both prof0) and student1 (exam in course1, prof0) qualify;
        //student2 only took course2 (prof1) and student3 took no exam => 2 results
        Assert.AreEqual(2, result.SelectResultsCount);
        List<string> keptStudents = result.SelectResults.Rows.Cast<DataRow>().Select(r => r["?S"].ToString()).OrderBy(s => s).ToList();
        CollectionAssert.AreEqual(new List<string> { $"{UniversityNamespace}student0", $"{UniversityNamespace}student1" }, keptStudents);
    }

    [TestMethod]
    public void ShouldAnswerQ15ValuesInlineWithMultiPatternExists()
    {
        //Q15 - orthogonal coverage of two seldom-exercised constructs together: an inline VALUES data block
        //restricting ?S to {student2, student3}, plus a multi-pattern EXISTS keeping only those with an exam.
        const string expectedQueryString = @"SELECT *
WHERE {
  {
    VALUES ?S { <http://uni.org/student2> <http://uni.org/student3> } .
    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://uni.org/Student> .
    FILTER ( EXISTS { ?EX <http://uni.org/examStudent> ?S . ?EX <http://uni.org/examCourse> ?COURSE . } ) 
  }
}
";
        RDFSelectQueryResult result = AssertQueryStringAndApply(expectedQueryString, new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddValues(new RDFValues().AddColumn(Variable("?s"), new List<RDFPatternMember> { UniversityResource("student2"), UniversityResource("student3") }))
                .AddPattern(new RDFPattern(Variable("?s"), RDFVocabulary.RDF.TYPE, UniversityResource("Student")))
                .AddFilter(new RDFExistsFilter(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(Variable("?ex"), UniversityResource("examStudent"), Variable("?s")))
                    .AddPattern(new RDFPattern(Variable("?ex"), UniversityResource("examCourse"), Variable("?course")))))));

        //Of {student2, student3}, only student2 has an exam => 1 result
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.AreEqual($"{UniversityNamespace}student2", result.SelectResults.Rows[0]["?S"].ToString());
    }
    #endregion

    #region Tests (parser end-to-end on the most recent phases F6/F7/F8.1)
    //These three tests exercise an axis the Q01..Q13 suite never touches: instead of building the query through
    //the fluent object model, they PARSE a raw SPARQL 1.1 string with the new RDFQueryParser and then EXECUTE it
    //against the very same university graph, asserting on the result set. They therefore validate the newest
    //design phases end-to-end — F6 (FILTER + built-in expressions), F7 (projection '(expr AS ?v)') and
    //F8.1 (BIND + VALUES inline data) — through the string -> object-model -> engine pipeline, not just the model.

    [TestMethod]
    public void ShouldParseAndAnswerFilterWithRegexBuiltIn()
    {
        //F6 - FILTER carrying the REGEX + STR built-ins, parsed from text and evaluated by the engine.
        //Course names are "Course 0".."Course 3"; the anchored class [13]$ keeps only those ending in 1 or 3.
        RDFSelectQuery query = RDFSelectQuery.FromString(@"
            PREFIX uni: <http://uni.org/>
            SELECT ?C WHERE {
                ?C a uni:Course .
                ?C uni:name ?N .
                FILTER(REGEX(STR(?N), ""[13]$""))
            }");

        RDFSelectQueryResult result = query.ApplyToGraph(UniversityGraph);

        //Only course1 ("Course 1") and course3 ("Course 3") satisfy the regex
        Assert.AreEqual(2, result.SelectResultsCount);
        HashSet<string> matchedCourses = new HashSet<string>(StringComparer.Ordinal);
        foreach (DataRow resultRow in result.SelectResults.Rows)
            matchedCourses.Add(resultRow["?C"].ToString());
        Assert.IsTrue(matchedCourses.Contains($"{UniversityNamespace}course1"));
        Assert.IsTrue(matchedCourses.Contains($"{UniversityNamespace}course3"));
    }

    [TestMethod]
    public void ShouldParseAndAnswerProjectionExpression()
    {
        //F7 - a computed projection '(?CR * 2 AS ?DOUBLE)', parsed from text and evaluated by the engine.
        RDFSelectQuery query = RDFSelectQuery.FromString(@"
            PREFIX uni: <http://uni.org/>
            SELECT ?CR (?CR * 2 AS ?DOUBLE) WHERE {
                ?C a uni:Course .
                ?C uni:credits ?CR
            }");

        RDFSelectQueryResult result = query.ApplyToGraph(UniversityGraph);

        //One row per course (4); each carries the doubled credits in the computed column
        Assert.AreEqual(4, result.SelectResultsCount);
        foreach (DataRow resultRow in result.SelectResults.Rows)
            Assert.AreEqual(GetNumericValue(resultRow["?CR"]) * 2, GetNumericValue(resultRow["?DOUBLE"]));
    }

    [TestMethod]
    public void ShouldParseAndAnswerValuesRestrictedBindOnAge()
    {
        //F8.1 - VALUES inline data restricting the subject to two named students, plus a BIND computing a
        //derived column, both parsed from text and evaluated by the engine.
        RDFSelectQuery query = RDFSelectQuery.FromString(@"
            PREFIX uni: <http://uni.org/>
            SELECT ?S ?A ?BONUS WHERE {
                VALUES ?S { uni:student0 uni:student2 }
                ?S uni:age ?A .
                BIND(?A * 2 AS ?BONUS)
            }");

        RDFSelectQueryResult result = query.ApplyToGraph(UniversityGraph);

        //VALUES keeps only student0 (age 20 => bonus 40) and student2 (age 30 => bonus 60)
        Assert.AreEqual(2, result.SelectResultsCount);
        Dictionary<string, double> bonusByStudent = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (DataRow resultRow in result.SelectResults.Rows)
        {
            Assert.AreEqual(GetNumericValue(resultRow["?A"]) * 2, GetNumericValue(resultRow["?BONUS"]));
            bonusByStudent.Add(resultRow["?S"].ToString(), GetNumericValue(resultRow["?BONUS"]));
        }
        Assert.AreEqual(40, bonusByStudent[$"{UniversityNamespace}student0"]);
        Assert.AreEqual(60, bonusByStudent[$"{UniversityNamespace}student2"]);
    }

    [TestMethod]
    public void ShouldParseAndAnswerCombinedRecentInnovations()
    {
        //A single query that crosses the most recent design phases end-to-end (string -> object-model -> engine),
        //and incidentally drives the resurrected pure-inner join fast-path:
        // - the 3-pattern BGP is fully bound and non-optional, so CombineTables takes InnerJoinTables (the fast-path);
        // - FILTER carries a string built-in (STRSTARTS over STR()) — an IP-era expression in a bare boolean filter;
        // - GROUP BY + COUNT/AVG with a FREE HAVING that references the aggregate AVG(?G) directly (IP3.3);
        // - ORDER BY on the aggregate alias plus the complete CONSTRUCT/SELECT modifier handling (IP4);
        // - a TRAILING query-level VALUES clause (IP5.1), joined with the WHERE solutions BEFORE the modifiers,
        //   which is what actually drops course0 before grouping.
        RDFSelectQuery query = RDFSelectQuery.FromString(@"
            PREFIX uni: <http://uni.org/>
            SELECT ?C (COUNT(?E) AS ?CNT) (AVG(?G) AS ?AVG) WHERE {
                ?E a uni:Exam .
                ?E uni:examCourse ?C .
                ?E uni:grade ?G .
                FILTER(STRSTARTS(STR(?C), STR(uni:course)))
            }
            GROUP BY ?C
            HAVING(AVG(?G) >= 20)
            ORDER BY DESC(?AVG)
            VALUES ?C { uni:course1 uni:course2 }");

        RDFSelectQueryResult result = query.ApplyToGraph(UniversityGraph);

        //Exams per course: course0=[25], course1=[28,18], course2=[30,22]. The trailing VALUES restricts to
        //course1/course2 (course0 dropped BEFORE grouping); both survive HAVING avg>=20; ORDER BY DESC(?AVG)
        //yields course2 (avg 26) first, then course1 (avg 23).
        Assert.AreEqual(2, result.SelectResultsCount);

        Assert.AreEqual($"{UniversityNamespace}course2", result.SelectResults.Rows[0]["?C"].ToString());
        Assert.AreEqual(2, GetNumericValue(result.SelectResults.Rows[0]["?CNT"]));
        Assert.AreEqual(26, GetNumericValue(result.SelectResults.Rows[0]["?AVG"]));

        Assert.AreEqual($"{UniversityNamespace}course1", result.SelectResults.Rows[1]["?C"].ToString());
        Assert.AreEqual(2, GetNumericValue(result.SelectResults.Rows[1]["?CNT"]));
        Assert.AreEqual(23, GetNumericValue(result.SelectResults.Rows[1]["?AVG"]));
    }
    #endregion
}
