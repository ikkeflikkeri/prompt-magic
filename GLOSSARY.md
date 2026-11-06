# Prompt Magic

Turns a rough idea into a well-structured prompt that a user can take to another AI model.

## Language

**Prompt**:
The finished text Prompt Magic produces for the user to take away and give to an AI model.
_Avoid_: Result, output, generated text

**Request**:
What the user submits to get a Prompt: a Goal, plus an optional Audience and Tone.
_Avoid_: Prompt request, input, query

**Goal**:
The part of a Request that says what the user wants the Target Model to accomplish. Required.
_Avoid_: Task, objective, instruction

**Audience**:
The part of a Request that says who will read the Target Model's answer. Optional.
_Avoid_: Reader, user persona

**Tone**:
The part of a Request that says what manner of writing the Target Model's answer should use. Optional.
_Avoid_: Style, voice, register

**Generating Model**:
The AI model Prompt Magic calls to write the Prompt.
_Avoid_: LLM, language model, upstream, "the model" (unqualified)

**Target Model**:
The AI model the user will later give the Prompt to. Chosen by the user outside Prompt Magic.
_Avoid_: Downstream model, "another AI model"

**Generator**:
The component that turns a Request into a Prompt. Either a Template Generator or a Model Generator.
_Avoid_: Builder, engine

**Template Generator**:
A Generator that wraps the Request in fixed sections and stock instructions, offline, without calling a Generating Model.
_Avoid_: Offline generator, fallback generator

**Model Generator**:
A Generator that has the Generating Model rewrite the Request into a Prompt.
_Avoid_: LLM generator, AI generator

**Generator Instructions**:
The operator-configured instructions that tell the Generating Model how to turn a Request into a Prompt. Not user-facing and not a Prompt itself.
_Avoid_: System prompt (in conversation; the code setting keeps that name)
