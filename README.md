# CsharpSemanticMerger
A semantic git conflict resolution engine and Visual Studio extension specifically designed for C# structures.

Traditional diff tools treat source as raw text lines, often breaking syntax ('<<<<<<< HEAD') and causing compile-time errors during complex merges. This tool understands the C# abstract syntax tree (AST) to resolve conflicts on a **method, property, and class level**.

---

## The Problem
When merging branches in large C# classes, text-based merge tool frequently create messy conflicts. It is easy to accidentally delete a closing brace '}' or mix up overloaded methods, leading to broken builds and wasted development time.

## Features
* **Semantic Analysis:** Uses the Microsoft Roslyn Compiler API to parse conflicting files into valid syntax trees.
* **Method-Level Diffing:** Instead of raw lines, see exactly whoch method or property changed ('HEAD' vs. 'Incoming').
* **One-Click Merge:** Safely merge or replace whole structural elements without worrying aboutmissing braces or namespace errors.
* **Visual Studio Integration:** Docks seamlessly into your daily workflow as a VSIX extension.

---

## Architecture & Tech Stack
To mantain clean code and testabillity, the project is strictly separated into three layers:

* **´.NET 10 Core Engine´:** Pure, independent class liberty containing the Roslyn parsing logic and the merge engine.
* **'xUnit Test Suite':** Comprehensive unit tests simulating complex edge cases nested classes, and broken syntax.
* **'VSIX Extension (WPF)':** The Visual Studio user interface, built with WPF/XAML and the Visual Studio SDK.

## Project Roadmap

## 🗺️ Project Roadmap

### Phase 1: Text & Git Parsing
* [ ] Split raw git conflict markers into distinct, valid in-memory code blocks.
* [ ] Maintain source line mapping for precise error reporting.

### Phase 2: Roslyn Syntax Analysis
* [ ] Extract methods, properties, and constructors using `SyntaxWalker`.
* [ ] Implement signature matching algorithms (handling method overloads).

### Phase 3: Visual Studio Integration & UI
* [ ] Build the side-by-side WPF comparison tool window.
* [ ] Implement the file-rewriter to cleanly save resolved code back to the editor.
