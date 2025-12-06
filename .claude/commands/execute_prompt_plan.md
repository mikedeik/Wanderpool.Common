Execute a prompt plan to implement unfinished prompts in the project

1. Read the **PRACTICES.md** file for paterns or constants you need to use
2. Open **@/prompt_plan.md** and identify any prompts not marked as completed.
3. Consult **USER_STORIES.md** to understand requirements and context for each unfinished prompt.
   For each incomplete prompt:
   - Double check if it's trully unfinished
   - If you confirm that it's allready done skip it.
   - Otherwise, implement as described
   - Create a new git branch named as features/{step-id}
   - Make sure the tests pass, and the program builds
   - Make sure all the tests pass, and the solution builds
   - Commit the changes to your repository with a clear commit message
   - merge into the develop branch
   - Update **@prompt_plan.md** to mark this prompt as completed.
4. Repeat with the next unfinished prompt
   No newline at end of file
5. Ask for review before merging into the development branch
   - If successful Update **@prompt_plan.md** to mark this prompt as completed.
6. Repeat with the next unfinished prompt