#include <iostream>
#include <vector>
#include <chrono>
#include <thread>
#include "rclcpp/rclcpp.hpp"
#include "std_msgs/msg/string.hpp"
#include "std_msgs/msg/int32.hpp"
#include "sensor_msgs/msg/image.hpp"
#include "sensor_msgs/msg/laser_scan.hpp"
#include "nav_msgs/msg/odometry.hpp"
#include "geometry_msgs/msg/transform_stamped.hpp"
#include "geometry_msgs/msg/twist.hpp"
#include "tf2_ros/transform_broadcaster.h"

#include "interface.h"


using namespace std::chrono_literals;
using std::placeholders::_1;


class MinimalPublisher : public rclcpp::Node {
public:
    MinimalPublisher() : Node("unity_ros_interface_publisher_node") {
        odom_publisher_ = create_publisher<nav_msgs::msg::Odometry>("odom", 10);
        tf_broadcaster_ = std::make_unique<tf2_ros::TransformBroadcaster>(*this);

        sub_node_ = std::make_unique<Node>("unity_ros_interface_subscriber_node");
        cmd_vel_subscriber_ = sub_node_->create_subscription<geometry_msgs::msg::Twist>("cmd_vel", 10, std::bind(
                &MinimalPublisher::cmd_vel_callback, this, _1));

        spin_thread_ = std::make_shared<std::thread>([this]() {
                                                         while (true) {
                                                             spin_some(sub_node_);
                                                             std::lock_guard<std::mutex> lock(mutex_);
                                                             if (shutdown) {
                                                                 break;
                                                             }
                                                         }
                                                     }
        );

    }

    ~MinimalPublisher() {
        std::lock_guard<std::mutex> lock(mutex_);
        shutdown = true;
        spin_thread_->join();
    }

    void publishTF(const NativeTransform *input) {
        geometry_msgs::msg::TransformStamped t;

        t.header.stamp = this->get_clock()->now();
        t.child_frame_id = input->frame_id;
        t.header.frame_id = input->child_frame_id;
        t.transform.translation.x = input->translation.x;
        t.transform.translation.y = input->translation.y;
        t.transform.translation.z = input->translation.z;
        t.transform.rotation.x = input->rotation.x;
        t.transform.rotation.y = input->rotation.y;
        t.transform.rotation.z = input->rotation.z;
        t.transform.rotation.w = input->rotation.w;

        tf_broadcaster_->sendTransform(t);
    }

    void PublishInt32(const NativeInt32 *input) {
        if (int32_publisher_.find(input->topic) == int32_publisher_.end()) {
            int32_publisher_[input->topic] = create_publisher<std_msgs::msg::Int32>(input->topic, 10);
        }
        auto message = std_msgs::msg::Int32();
        message.data = input->data;
        int32_publisher_[input->topic]->publish(message);
    }

    void PublishImage(const NativeImage *input) {
        if (image_publisher_.find(input->topic) == image_publisher_.end()) {
            image_publisher_[input->topic] = create_publisher<sensor_msgs::msg::Image>(input->topic, 10);
        }
        auto message = sensor_msgs::msg::Image();
        message.header.stamp = this->get_clock()->now();
        message.header.frame_id = input->frame_id;

        message.height = input->height;
        message.width = input->width;
        message.encoding = input->encoding;
        message.step = input->step;
        int len = input->height * input->step;
        message.data = std::vector<uint8_t>(input->data, input->data + len);
        image_publisher_[input->topic]->publish(message);
    }

    void PublishScan(const NativeScan *input) {
        if (scan_publisher_.find(input->topic) == scan_publisher_.end()) {
            scan_publisher_[input->topic] = create_publisher<sensor_msgs::msg::LaserScan>(input->topic, 10);
        }
        auto message = sensor_msgs::msg::LaserScan();
        message.header.stamp = this->get_clock()->now();
        message.header.frame_id = input->frame_id;
        message.angle_min = input->angle_min;
        message.angle_max = input->angle_max;
        message.angle_increment = input->angle_increment;
        message.time_increment = input->time_increment*0;
        message.scan_time = input->scan_time*0;
        message.range_min = input->range_min;
        message.range_max = input->range_max;
        message.ranges = std::vector<float>(input->ranges, input->ranges + input->count);
        message.intensities = std::vector<float>(input->intensities, input->intensities + input->count);
        scan_publisher_[input->topic]->publish(message);
    }

    void PublishOdom(const NativeOdom *input) {
        auto message = nav_msgs::msg::Odometry();
        message.header.stamp = this->get_clock()->now();
        message.header.frame_id = input->pose.frame_id;
        message.child_frame_id = input->pose.child_frame_id;

        message.pose.pose.position.x = input->pose.translation.x;
        message.pose.pose.position.y = input->pose.translation.y;
        message.pose.pose.position.z = input->pose.translation.z;
        message.pose.pose.orientation.x = input->pose.rotation.x;
        message.pose.pose.orientation.y = input->pose.rotation.y;
        message.pose.pose.orientation.z = input->pose.rotation.z;
        message.pose.pose.orientation.w = input->pose.rotation.w;
        std::memcpy(&message.pose.covariance, input->pose_covariance, sizeof(input->pose_covariance));

        message.twist.twist.linear.x = input->linear_velocity.x;
        message.twist.twist.linear.y = input->linear_velocity.y;
        message.twist.twist.linear.z = input->linear_velocity.z;

        message.twist.twist.angular.x = input->angular_velocity.x;
        message.twist.twist.angular.y = input->angular_velocity.y;
        message.twist.twist.angular.z = input->angular_velocity.z;

        std::memcpy(&message.twist.covariance, input->twist_covariance, sizeof(input->twist_covariance));
        odom_publisher_->publish(message);
    }

    geometry_msgs::msg::Twist cmd_vel;
    std::mutex mutex_;
    bool shutdown = false;

private:
    void cmd_vel_callback(const geometry_msgs::msg::Twist::SharedPtr msg) {
        std::lock_guard<std::mutex> lock(mutex_);
        cmd_vel = *msg;
    }

    rclcpp::TimerBase::SharedPtr timer_;
    std::unordered_map<std::string, rclcpp::Publisher<std_msgs::msg::Int32>::SharedPtr> int32_publisher_;
    std::unordered_map<std::string, rclcpp::Publisher<sensor_msgs::msg::Image>::SharedPtr> image_publisher_;
    std::unordered_map<std::string, rclcpp::Publisher<sensor_msgs::msg::LaserScan>::SharedPtr> scan_publisher_;
    rclcpp::Publisher<nav_msgs::msg::Odometry>::SharedPtr odom_publisher_;
    std::unique_ptr<tf2_ros::TransformBroadcaster> tf_broadcaster_;

    rclcpp::Subscription<geometry_msgs::msg::Twist>::SharedPtr cmd_vel_subscriber_;
    Node::SharedPtr sub_node_;
    std::shared_ptr<std::thread> spin_thread_;

};

class ROSInterface {
public:

    ROSInterface() {
        auto name = "ros_node";
        rclcpp::init(1, &name);
        node_ = std::make_shared<MinimalPublisher>();
    }

    ~ROSInterface() {
        std::lock_guard<std::mutex> lock(node_->mutex_);
        node_->shutdown = true;
        rclcpp::shutdown();
    }

    void PublishTF(NativeTransform *input) {
        node_->publishTF(input);
    }

    void PublishInt32(NativeInt32 *input) {
        node_->PublishInt32(input);
    }

    void PublishOdom(NativeOdom *input) {
        node_->PublishOdom(input);
    }

    void PublishImage(NativeImage *input) {
        node_->PublishImage(input);
    }

    void PublishScan(NativeScan *input) {
        node_->PublishScan(input);
    }


    void ReceiveCmdVel(NativeTwist *output) {
        std::lock_guard<std::mutex> lock(node_->mutex_);
        output->linear.x = node_->cmd_vel.linear.x;
        output->linear.y = node_->cmd_vel.linear.y;
        output->linear.z = node_->cmd_vel.linear.z;
        output->angular.x = node_->cmd_vel.angular.x;
        output->angular.y = node_->cmd_vel.angular.y;
        output->angular.z = node_->cmd_vel.angular.z;
    }


    std::shared_ptr<MinimalPublisher> node_;

};

std::intptr_t Init() {
    return (std::intptr_t) new ROSInterface();
}

void Destroy(std::intptr_t handle) {
    auto ptr = (ROSInterface *) handle;
    delete ptr;
}

void PublishTF(std::intptr_t handle, NativeTransform *input) {
    if (handle != 0) {
        auto ptr = (ROSInterface *) handle;
        ptr->PublishTF(input);
    }
}

void PublishInt32(std::intptr_t handle, NativeInt32 *input) {
    if (handle != 0) {
        auto ptr = (ROSInterface *) handle;
        ptr->PublishInt32(input);
    }
}

void PublishOdom(std::intptr_t handle, NativeOdom *input) {
    if (handle != 0) {
        auto ptr = (ROSInterface *) handle;
        ptr->PublishOdom(input);
    }
}

void PublishImage(std::intptr_t handle, NativeImage *input) {
    if (handle != 0) {
        auto ptr = (ROSInterface *) handle;
        ptr->PublishImage(input);
    }
}

void PublishScan(std::intptr_t handle, NativeScan *input) {
    if (handle != 0) {
        auto ptr = (ROSInterface *) handle;
        ptr->PublishScan(input);
    }
}


void ReceiveCmdVel(std::intptr_t handle, NativeTwist *output) {
    if (handle != 0) {
        auto ptr = (ROSInterface *) handle;
        ptr->ReceiveCmdVel(output);
    }
}

