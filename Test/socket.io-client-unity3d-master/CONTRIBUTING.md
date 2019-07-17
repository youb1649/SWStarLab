# Contributing to Socket.IO-Client-Unity3D
Contributions are more than welcome, so thank you for wanting to contribute to Socket.IO-Client-Unity3D!

---

### Checklist before creating a Pull Request
Submit only relevant commits. We don't mind many commits in a pull request, but they must be relevant as explained below.

- __Use a feature branch__ The pull request should be created from a feature branch, and not from _develop_. See below for why.
- __No merge-commits__
If you have commits that looks like this _"Merge branch 'my-branch' into develop"_ or _"Merge branch 'develop' of https://github.com/nhnent/socket.io-client-unity3d into develop"_ you're probaly using merge instead of [rebase](https://help.github.com/articles/about-git-rebase) locally. See below on _Handling updates from upstream_.
- __Squash commits__ Often we create temporary commits like _"Started implementing feature x"_ and then _"Did a bit more on feature x"_. Squash these commits together using [interactive rebase](https://help.github.com/articles/about-git-rebase). Also see [Squashing commits with rebase](http://gitready.com/advanced/2009/02/10/squashing-commits-with-rebase.html).
- __Descriptive commit messages__ If a commit's message isn't descriptive, change it using [interactive rebase](https://help.github.com/articles/about-git-rebase). Refer to issues using `#issue`. Example of a bad message ~~"Small cleanup"~~. Example of good message: _"Removed Security.Claims header from FSM, which broke Mono build per #62"_. Don't be afraid to write long messages, if needed. Try to explain _why_ you've done the changes. The Erlang repo has some info on [writing good commit messages](https://github.com/erlang/otp/wiki/Writing-good-commit-messages).
- __No one-commit-to-rule-them-all__ Large commits that changes too many things at the same time are very hard to review. Split large commits into smaller. See this [StackOverflow question](http://stackoverflow.com/questions/6217156/break-a-previous-commit-into-multiple-commits) for information on how to do this.
- __Tests__ Add relevant tests and make sure all existing ones still passes. Tests can be run using the command
- __No Warnings__ Make sure your code do not produce any build warnings.

After reviewing a Pull request, we might ask you to fix some commits. After you've done that you need to force push to update your branch in your local fork.

####Title and Description for the Pull Request####
Give the PR a descriptive title and in the description field describe what you have done in general terms and why. This will help the reviewers greatly, and provide a history for the future.

Especially if you modify something existing, be very clear! Have you changed any algorithms, or did you just intend to reorder the code? Justify why the changes are needed.


---

### Getting started
Make sure you have a [GitHub](https://github.com/) account.

- Fork, clone, add upstream to the Socket.io-Client-Unity3D framework repository. See [Fork a repo](https://help.github.com/articles/fork-a-repo) for more detailed instructions or follow the instructions below.

- Fork by clicking _Fork_ on https://github.com/nhnent/socket.io-client-unity3d
- Clone your fork locally.
```
git clone https://github.com/YOUR-USERNAME/socket.io-client-unity3d
```
- Add an upstream remote.
```
git remote add upstream https://github.com/nhnent/socket.io-client-unity3d
```
You now have two remotes: _upstream_ points to https://github.com/nhnent/socket.io-client-unity3d, and _origin_ points to your fork on GitHub.

- Make changes. See below.

See also: [Contributing to Open Source on GitHub](https://guides.github.com/activities/contributing-to-open-source/)

New to Git? See https://help.github.com/articles/what-are-other-good-resources-for-learning-git-and-github

### Making changes
__Never__ work directly on _develop_ or _master_ and you should never send a pull request from master - always from a feature branch created by you.

- Pick an [issue](https://github.com/nhnent/socket.io-client-unity3d/issues). If no issue exists (search first) create one.
-  Get any changes from _upstream_.
```
git checkout develop
git fetch upstream
git merge --ff-only upstream/develop
git push origin develop     #(optional) this makes sure develop in your own fork on GitHub is up to date
```

See https://help.github.com/articles/fetching-a-remote for more info

- Create a new feature branch. It's important that you do your work on your own branch and that it's created off of _develop_. Tip: Give it a descriptive name and include the issue number, e.g. `implement-lengthframeencoder-323` or `295-implement-recvbuffer`, so that others can see what is being worked on.
```
git checkout -b my-new-branch-123
```
- Work on your feature. Commit.
- Rebase often, see below.
- Make sure you adhere to _Checklist before creating a Pull Request_ described above.
- Push the branch to your fork on GitHub
```
git push origin my-new-branch-123
```
- Send a Pull Request, see https://help.github.com/articles/using-pull-requests to the _develop_ branch.

See also: [Understanding the GitHub Flow](https://guides.github.com/introduction/flow/) (we're using `develop` as our master branch)

### Handling updates from upstream

While you're working away in your branch it's quite possible that your upstream _develop_ may be updated. If this happens you should:

- [Stash](http://git-scm.com/book/en/Git-Tools-Stashing) any un-committed changes you need to
```
git stash
```
- Update your local _develop_ by fetching from _upstream_
```
git checkout develop
git fetch upstream
git merge --ff-only upstream/develop
```
- Rebase your feature branch on _develop_. See [Git Branching - Rebasing](http://git-scm.com/book/en/Git-Branching-Rebasing) for more info on rebasing
```
git checkout my-new-branch-123
git rebase develop
git push origin develop     #(optional) this makes sure develop in your own fork on GitHub is up to date
```
This ensures that your history is "clean" i.e. you have one branch off from _develop_ followed by your changes in a straight line. Failing to do this ends up with several "messy" merges in your history, which we don't want. This is the reason why you should always work in a branch and you should never be working in, or sending pull requests from _develop_.

If you're working on a long running feature then you may want to do this quite often, rather than run the risk of potential merge issues further down the line.

### Making changes to a Pull request
If you realize you've missed something after submitting a Pull request, just commit to your local branch and push the branch just like you did the first time. This commit will automatically be included in the Pull request.
If we ask you to change already published commits using interactive rebase (like squashing or splitting commits or  rewriting commit messages) you need to force push using `-f`:
```
git push -f origin my-new-branch-123
```

### All my commits are on develop. How do I get them to a new branch? ###
If all commits are on _develop_ you need to move them to a new feature branch.

You can rebase your local _develop_ on _upstream/develop_ (to remove any merge commits), rename it, and recreate _develop_
```
git checkout develop
git rebase upstream/develop
git branch -m my-new-branch-123
git branch develop upstream/develop
```
Or you can create a new branch off of _develop_ and then cherry pick the commits
```
git checkout -b my-new-branch-123 upstream/develop
git cherry-pick rev           #rev is the revisions you want to pick
git cherry-pick rev           #repeat until you have picked all commits
git branch -m develop old-develop     #rename develop
git branch develop upstream/develop   #create a new develop
```

### Submitting Pull Requests

Before changes can be accepted a Contributor Licensing Agreement for Individual | Corporate must be completed.

* If you are an individual writing original source code and you're sure you own the intellectual property, then you'll need to sign an [Individual CLA](https://docs.google.com/forms/d/1MhnsLF2VxhqnRp2fwifXxcpCsoa193T7RKthq4E9KEs/viewform?edit_requested=true )
* If you work for a company that wants to allow you to contribute your work, then you'll need to sign a [Corporate CLA](https://docs.google.com/forms/d/1z-TB_Q6ll7Q1-kft3gQp9mPdC1wfxDVP3u0_nyYJE9g/viewform?edit_requested=true )
* If you are an representative writing original source code, then you'll need to sign a [Representative CLA](https://docs.google.com/forms/d/14JqWub7w2Tw-LjBIs0viJFah5vwjRtNKCNhHz9Pnnpw/viewform?edit_requested=true )

For Koreans(한국인)
* If you are an individual writing original source code and you're sure you own the intellectual property, then you'll need to sign an [Individual CLA](https://docs.google.com/forms/d/e/1FAIpQLSekugNvCBPl7N2M5ONqHG4Lw1fqjfHzw1Lq-Zzm509LlKH7uA/viewform )
* If you work for a company that wants to allow you to contribute your work, then you'll need to sign a [Corporate CLA](https://docs.google.com/forms/d/e/1FAIpQLScEnxqarG8t8DVbsXaXQTSrXpxvhKyrlJi-EjUOiDrlxcC0Zg/viewform )
* If you are an representative writing original source code, then you'll need to sign a [Representative CLA](https://docs.google.com/forms/d/14S9lgd_DxsUhKNyMtatefPusuUSOiIdBYCAWrZygogU/viewform?edit_requested=true )

Have a question for us? please send e-mail at oss@nhnent.com.

---
Props to [Akka.NET](http://getakka.net/) and [NancyFX](https://github.com/NancyFx/Nancy), and [DotNetty](https://github.com/Azure/DotNetty) from which we've "borrowed" this text.
