import os
import requests
import json
import subprocess

def get_pr_diff():
    """Get the diff of the current PR"""
    cmd = ["git", "diff", "origin/main...HEAD"]
    result = subprocess.run(cmd, capture_output=True, text=True)
    return result.stdout

def call_gemini_api(diff_content):
    """Call Google Gemini API for code review"""
    api_key = os.getenv('GEMINI_API_KEY')
    
    prompt = f"""
    Please review the following code changes and provide feedback focusing on:
    1. Code quality and best practices
    2. Potential bugs or security issues
    3. Performance improvements
    4. Code readability and maintainability
    
    Provide specific, actionable feedback. If the code looks good, say so.
    
    Code changes:
    {diff_content}
    """
    
    url = f"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={api_key}"
    
    payload = {
        "contents": [{
            "parts": [{
                "text": prompt
            }]
        }]
    }
    
    response = requests.post(url, json=payload)
    
    if response.status_code == 200:
        return response.json()['candidates'][0]['content']['parts'][0]['text']
    else:
        return f"Error calling Gemini API: {response.status_code}"

def post_comment_to_pr(comment):
    """Post comment to the PR"""
    github_token = os.getenv('GITHUB_TOKEN')
    pr_number = os.getenv('PR_NUMBER')
    repo_name = os.getenv('REPO_NAME')
    
    url = f"https://api.github.com/repos/{repo_name}/issues/{pr_number}/comments"
    
    headers = {
        "Authorization": f"token {github_token}",
        "Accept": "application/vnd.github.v3+json"
    }
    
    data = {
        "body": f"## 🤖 AI Code Review\n\n{comment}"
    }
    
    response = requests.post(url, headers=headers, json=data)
    return response.status_code == 201

def main():
    # Get PR diff
    diff = get_pr_diff()
    
    if not diff.strip():
        print("No code changes found")
        return
    
    # Get AI review
    review = call_gemini_api(diff)
    
    # Post comment
    if post_comment_to_pr(review):
        print("AI review posted successfully")
    else:
        print("Failed to post AI review")

if __name__ == "__main__":
    main()
