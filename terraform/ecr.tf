# VoteApp Infrastructure - ECR Repositories
# ==========================================
# ECR = Elastic Container Registry (AWS Docker Hub)
# Think of it as: A private photo album for your Docker images
# Only your AWS account can access these images (security!)

# =============================================================================
# ECR Repositories - One for each microservice
# Think of it as: Separate folders for each service's container images
# =============================================================================

# Vote Service - Python Flask frontend
resource "aws_ecr_repository" "vote" {
  name                 = "${var.project_name}/vote"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true  # DevSecOps: Automatically scan images for vulnerabilities
  }

  tags = {
    Name    = "${var.project_name}-vote"
    Service = "vote"
  }
}

# Result Service - Node.js results dashboard
resource "aws_ecr_repository" "result" {
  name                 = "${var.project_name}/result"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name    = "${var.project_name}-result"
    Service = "result"
  }
}

# Worker Service - .NET Core vote processor
resource "aws_ecr_repository" "worker" {
  name                 = "${var.project_name}/worker"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name    = "${var.project_name}-worker"
    Service = "worker"
  }
}

# =============================================================================
# ECR Lifecycle Policy - Cleanup Old Images
# Think of it as: Automatic cleanup to avoid paying for unused images
# =============================================================================
resource "aws_ecr_lifecycle_policy" "cleanup" {
  for_each = {
    vote   = aws_ecr_repository.vote.name
    result = aws_ecr_repository.result.name
    worker = aws_ecr_repository.worker.name
  }

  repository = each.value

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Keep only last 10 images"
      selection = {
        tagStatus   = "any"
        countType   = "imageCountMoreThan"
        countNumber = 10
      }
      action = {
        type = "expire"
      }
    }]
  })
}
