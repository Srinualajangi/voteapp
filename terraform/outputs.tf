# VoteApp Infrastructure - Terraform Outputs
# ===========================================
# Outputs are values that Terraform displays after apply
# Think of it as: The receipt showing what was created

output "vpc_id" {
  description = "ID of the VPC"
  value       = aws_vpc.main.id
}

output "public_subnet_ids" {
  description = "IDs of the public subnets"
  value       = aws_subnet.public[*].id
}

output "private_subnet_ids" {
  description = "IDs of the private subnets"
  value       = aws_subnet.private[*].id
}

output "eks_cluster_name" {
  description = "Name of the EKS cluster"
  value       = aws_eks_cluster.main.name
}

output "eks_cluster_endpoint" {
  description = "Endpoint URL for EKS cluster API"
  value       = aws_eks_cluster.main.endpoint
}

output "eks_cluster_certificate" {
  description = "Certificate authority data for EKS cluster"
  value       = aws_eks_cluster.main.certificate_authority[0].data
  sensitive   = true
}

output "ecr_vote_url" {
  description = "ECR repository URL for vote service"
  value       = aws_ecr_repository.vote.repository_url
}

output "ecr_result_url" {
  description = "ECR repository URL for result service"
  value       = aws_ecr_repository.result.repository_url
}

output "ecr_worker_url" {
  description = "ECR repository URL for worker service"
  value       = aws_ecr_repository.worker.repository_url
}

# Command to configure kubectl
output "kubectl_config_command" {
  description = "Command to configure kubectl to connect to EKS"
  value       = "aws eks update-kubeconfig --name ${aws_eks_cluster.main.name} --region ${var.aws_region}"
}
