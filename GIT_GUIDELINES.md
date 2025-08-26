# Git Guidelines and Best Practices

## Table of Contents
1. [Commit Strategy](#commit-strategy)
2. [Commit Message Format](#commit-message-format)
3. [Branch Management](#branch-management)
4. [Pull Request Process](#pull-request-process)
5. [Git Workflow](#git-workflow)
6. [Common Scenarios](#common-scenarios)
7. [Git Commands Reference](#git-commands-reference)

---

## Commit Strategy

### Core Principle: One Commit Per Logical Component
Each commit should represent a complete, working piece of functionality that:
- ✅ Builds successfully
- ✅ Has all tests passing
- ✅ Maintains or improves test coverage
- ✅ Can be deployed independently

### Commit Granularity by Layer

#### Backend Services
```
Domain Layer     → 1 commit per aggregate
Application Layer → 1 commit per use case group
Infrastructure   → 1 commit per major component
API Layer        → 1 commit per controller
```

#### Frontend Applications
```
Setup           → 1-2 commits for configuration
Components      → 1 commit per component group
Pages           → 1 commit per page/route
State           → 1 commit per feature slice
```

### Example Commit Sequence (Week 2 Sprint)
```bash
# Day 1: Domain
feat: Add Product aggregate with specifications pattern
feat: Add Category aggregate with hierarchical structure

# Day 2: Application
feat: Add Product CQRS commands and handlers
feat: Add Product search queries with filtering

# Day 3: Infrastructure
feat: Add Product repository with EF Core
feat: Add Elasticsearch product indexing

# Day 4: More Domain/Application
feat: Add Listing aggregate with auction types
feat: Add Listing CQRS operations

# Day 5: API
feat: Add Products API controller
feat: Add Listings API controller
feat: Add PostGIS geospatial search
```

---

## Commit Message Format

### Standard Format
```
type(scope): subject

body (optional)

footer (optional)
```

### Types
- **feat**: New feature
- **fix**: Bug fix
- **test**: Adding or updating tests
- **refactor**: Code change that neither fixes a bug nor adds a feature
- **docs**: Documentation only changes
- **style**: Code style changes (formatting, missing semicolons, etc.)
- **perf**: Performance improvements
- **chore**: Maintenance tasks (updating dependencies, etc.)
- **ci**: CI/CD pipeline changes
- **build**: Build system changes

### Scope Examples
- `user`: User service related
- `product`: Product service related
- `auth`: Authentication related
- `api`: API layer changes
- `db`: Database related
- `ui`: Frontend UI components

### Subject Guidelines
- Use imperative mood ("Add feature" not "Added feature")
- Don't capitalize first letter
- No period at the end
- Maximum 50 characters

### Body Guidelines
- Wrap at 72 characters
- Explain *what* and *why*, not *how*
- Include motivation for change
- Contrast with previous behavior

### Footer Guidelines
- Reference issues: `Closes #123`
- Breaking changes: `BREAKING CHANGE: description`
- Co-authors: `Co-authored-by: Name <email>`

### Good Examples
```bash
feat(product): add elasticsearch indexing for products

Implement automatic indexing of products to Elasticsearch
when products are created or updated. This enables full-text
search capabilities across product names and descriptions.

- Add ISearchService interface
- Implement ElasticsearchService 
- Add background job for index sync
- Include integration tests

Closes #45
```

```bash
fix(auth): resolve token refresh race condition

Multiple simultaneous requests were causing token refresh
to fail due to race condition in refresh token validation.
Implemented mutex lock to ensure single refresh operation.

Fixes #89
```

### Bad Examples
```bash
# Too vague
update code

# Not imperative
Added new feature

# Too long subject
feat(listing): implement complete listing creation workflow with validation and error handling

# Missing type
User service updates
```

---

## Branch Management

### Branch Naming Convention
```
type/description
type/issue-number-description
```

### Branch Types
- **feature/**: New features
- **bugfix/**: Bug fixes (non-critical)
- **hotfix/**: Critical production fixes
- **release/**: Release preparation
- **chore/**: Maintenance tasks

### Examples
```bash
feature/week2-product-listing-services
feature/123-add-payment-integration
bugfix/456-fix-login-validation
hotfix/critical-payment-bug
release/v1.2.0
chore/update-dependencies
```

### Branch Lifecycle
1. Create from main
2. Regular commits during development
3. Keep updated with main (rebase preferred)
4. Create PR when ready
5. Merge after approval
6. Delete branch after merge

### Long-Running Branches
- **main**: Production-ready code
- **develop**: Integration branch (optional)
- **staging**: Pre-production testing (optional)

---

## Pull Request Process

### PR Creation Checklist
- [ ] All tests passing
- [ ] 100% test coverage for new code
- [ ] Code follows project conventions
- [ ] Commits are logical and well-formatted
- [ ] Branch is up-to-date with main
- [ ] PR description is complete

### PR Title Format
Same as commit message format:
```
type(scope): description
```

### PR Description Template
```markdown
## Summary
Brief description of changes

## Changes Made
- Bullet point list of changes
- Include technical details
- Mention architectural decisions

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed
- [ ] Test coverage: XX%

## Screenshots (if applicable)
Include for UI changes

## Breaking Changes
List any breaking changes

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex logic
- [ ] Documentation updated
- [ ] No console.logs or debug code

## Related Issues
Closes #XXX
```

### Review Process
1. **Self-review** first
2. **Request review** from team
3. **Address feedback** promptly
4. **Re-request review** after changes
5. **Merge** after approval

### Merge Strategy
- **Squash and merge**: For feature branches (recommended)
- **Merge commit**: For release branches
- **Rebase and merge**: For clean, linear history

---

## Git Workflow

### Daily Development Flow
```bash
# 1. Start day - update main
git checkout main
git pull origin main

# 2. Create/continue feature branch
git checkout -b feature/new-feature
# or
git checkout feature/existing-feature
git rebase main  # Keep updated

# 3. Development cycle (TDD)
# Write tests → Code → Commit
git add .
git commit -m "feat: add new component"

# 4. End of day - push changes
git push origin feature/new-feature
```

### TDD Commit Flow
```bash
# RED phase - failing tests
git add tests/
git commit -m "test: add tests for user validation"

# GREEN phase - make tests pass
git add src/
git commit -m "feat: implement user validation"

# REFACTOR phase - improve code
git add .
git commit -m "refactor: optimize validation logic"

# OR combine in single commit
git add .
git commit -m "feat: add user validation with tests"
```

### PR Workflow
```bash
# 1. Ensure branch is updated
git checkout feature/my-feature
git rebase main

# 2. Push branch
git push origin feature/my-feature

# 3. Create PR via GitHub CLI
gh pr create --title "feat: add payment integration" \
             --body "Description..." \
             --base main

# 4. After approval, merge
gh pr merge --squash --delete-branch
```

---

## Common Scenarios

### Scenario: Fixing a Mistake in Last Commit
```bash
# Amend last commit (before pushing)
git add .
git commit --amend

# If already pushed (use with caution)
git push --force-with-lease
```

### Scenario: Cleaning Up Multiple Commits
```bash
# Interactive rebase for last 3 commits
git rebase -i HEAD~3

# Mark commits to squash in editor
# pick abc1234 First commit
# squash def5678 Second commit  
# squash ghi9012 Third commit
```

### Scenario: Undoing Changes
```bash
# Undo last commit but keep changes
git reset --soft HEAD~1

# Undo last commit and discard changes
git reset --hard HEAD~1

# Undo specific file changes
git checkout -- file.txt
```

### Scenario: Stashing Work
```bash
# Stash current changes
git stash

# Apply stash
git stash pop

# List stashes
git stash list

# Apply specific stash
git stash apply stash@{2}
```

### Scenario: Cherry-Picking
```bash
# Apply specific commit to current branch
git cherry-pick abc1234

# Cherry-pick range
git cherry-pick abc1234..def5678
```

### Scenario: Resolving Conflicts
```bash
# During merge/rebase
git status  # See conflicted files

# Edit files to resolve conflicts
# Look for <<<<<<< ======= >>>>>>>

# After resolving
git add resolved-file.txt
git rebase --continue  # or git merge --continue
```

---

## Git Commands Reference

### Essential Commands
```bash
# Status and History
git status                    # Check working directory status
git log --oneline -10         # View recent commits
git diff                      # View unstaged changes
git diff --staged             # View staged changes

# Branching
git branch                    # List local branches
git branch -a                 # List all branches
git checkout -b feature/new   # Create and switch branch
git branch -d feature/old     # Delete local branch
git push origin --delete old  # Delete remote branch

# Remote Operations
git fetch origin              # Fetch remote changes
git pull origin main          # Pull and merge changes
git push origin feature       # Push branch to remote
git remote -v                 # View remote repositories

# Tagging
git tag v1.0.0                # Create lightweight tag
git tag -a v1.0.0 -m "msg"    # Create annotated tag
git push origin v1.0.0        # Push tag to remote
git tag -d v1.0.0             # Delete local tag

# Cleanup
git clean -fd                 # Remove untracked files/dirs
git gc                        # Garbage collection
git prune                     # Remove unreachable objects
```

### Advanced Commands
```bash
# Finding Issues
git bisect start              # Start binary search
git bisect bad                # Mark current as bad
git bisect good v1.0.0        # Mark v1.0.0 as good
git blame file.txt            # Show who changed each line

# Rewriting History
git rebase -i HEAD~5          # Interactive rebase
git filter-branch             # Rewrite branch history
git reflog                    # View reference log

# Submodules
git submodule add url path    # Add submodule
git submodule update --init   # Initialize submodules
git submodule foreach git pull # Update all submodules

# Performance
git count-objects -v          # View repository size
git fsck                      # Check repository integrity
git prune --expire=now        # Remove old objects
```

### Aliases (add to ~/.gitconfig)
```ini
[alias]
    st = status
    co = checkout
    br = branch
    ci = commit
    unstage = reset HEAD --
    last = log -1 HEAD
    visual = log --graph --oneline --all
    undo = reset --soft HEAD~1
```

---

## Best Practices Summary

### DO ✅
- Commit early and often
- Write descriptive commit messages
- Keep commits atomic and focused
- Test before committing
- Pull before pushing
- Use feature branches
- Delete merged branches
- Review your own PR first
- Keep main branch stable
- Use conventional commit format

### DON'T ❌
- Commit broken code
- Mix features in one commit
- Force push to shared branches
- Commit sensitive data
- Use generic commit messages
- Work directly on main
- Leave commented code
- Ignore merge conflicts
- Skip tests
- Rewrite public history

---

## Resources

- [Conventional Commits](https://www.conventionalcommits.org/)
- [Git Documentation](https://git-scm.com/doc)
- [GitHub Flow](https://guides.github.com/introduction/flow/)
- [Atlassian Git Tutorials](https://www.atlassian.com/git/tutorials)

---

*Last Updated: August 24, 2025*